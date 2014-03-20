using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;

namespace DBS
{
    /// <summary>
    /// Launches the chunk backup subprotocol
    /// </summary>
    public class BackupChunkSubprotocol : IProtocol
    {
        private readonly FileId _fileId;
        private readonly int _chunkNo;
        private readonly int _replicationDegree;
        private readonly byte[] _data;

        private const int InitialTimeout = 500;
        private const int MaxRetries = 5;

        public BackupChunkSubprotocol(FileId fileId, int chunkNo, int replicationDegree, byte[] data)
        {
            _fileId = fileId;
            _chunkNo = chunkNo;
            _replicationDegree = replicationDegree;
            _data = data;
        }

        private int _count { get { return _storedsFrom.Count; } }
        private readonly HashSet<string> _storedsFrom = new HashSet<string>();
        private void StoreHandler(Message msg)
        {
            _storedsFrom.Add(msg.RemoteEndPoint.ToString());
        }

        private void SendChunk()
        {
            var timeout = InitialTimeout;

            int retryCount;
            for (retryCount = 0; retryCount < MaxRetries; retryCount++)
            {
                var msg = Message.BuildPutChunkMessage(_fileId, _chunkNo, _replicationDegree, _data);
                Core.Instance.MDBChannel.Send(msg);

                Thread.Sleep(timeout);

                if (_count >= _replicationDegree)
                    break;

                timeout *= 2;
                if (retryCount != MaxRetries - 1) // not last iter
                    Console.WriteLine("[{3}#{4}: ChunkReplication degree is {0} but wanted {1}. Timeout increased to {2}",
                        _count, _replicationDegree, timeout, _fileId.ToString().Substring(0, 5), _chunkNo);
            }

            Console.WriteLine("{2}#{3}: Stored or giving up: retries {0}, rep degree {1}", retryCount, _count, _fileId.ToString().Substring(0, 5), _chunkNo);
            Core.Instance.Store.UpdateDegrees(_fileId + "_" + _chunkNo, _count, _replicationDegree);
        }

        public void Run()
        {
            var subs = Core.Instance.MCChannel.Received.Where(message =>
                message.MessageType == MessageType.Stored &&
                message.FileId == _fileId &&
                message.ChunkNo == _chunkNo).Subscribe(StoreHandler);

            //Task.Factory.StartNew(SendChunk).ContinueWith(task => subs.Dispose());
            SendChunk();
            subs.Dispose(); // unsubscribe
        }
    }
}

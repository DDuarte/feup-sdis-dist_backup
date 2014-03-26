using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DBS.Messages;

namespace DBS.Protocols
{
    /// <summary>
    /// Launches the chunk backup subprotocol
    /// </summary>
    public class BackupChunkSubprotocol : IProtocol
    {
        private readonly FileChunk _fileChunk;
        private readonly int _replicationDegree;
        private readonly byte[] _data;

        public BackupChunkSubprotocol(FileChunk fileChunk, int replicationDegree, byte[] data)
        {
            _fileChunk = fileChunk;
            _replicationDegree = replicationDegree;
            _data = data;
        }

        private int _count { get { return _storedsFrom.Count; } }
        private readonly HashSet<string> _storedsFrom = new HashSet<string>();
        private void StoreHandler(StoredMessage msg)
        {
            // HashSet.Add only adds unique objects
            // the idea here is to count multiple Stored messages from
            // the same peer only once
            _storedsFrom.Add(msg.VersionN + "_" + msg.RemoteEndPoint);

            // FIXME: Temporary hack: prepend version so it is possible to test
            // in the same computer with two peers using different version
        }

        private void SendChunk()
        {
            var timeout = Core.Instance.Config.BackupChunkTimeout;
            var maxRetries = Core.Instance.Config.BackupChunkRetries;
            var multi = Core.Instance.Config.BackupChunkTimeoutMultiplier;

            int retryCount;
            for (retryCount = 0; retryCount < maxRetries; retryCount++)
            {
                var msg = new PutChunkMessage(_fileChunk, _replicationDegree, _data);
                Core.Instance.MDBChannel.Send(msg);

                Thread.Sleep(timeout);

                if (_count >= _replicationDegree)
                    break;

                timeout = (int) (timeout * multi);
                if (retryCount != maxRetries - 1) // not last iter
                    Core.Instance.Log.InfoFormat("{0}: ChunkReplication degree is {1} but wanted {2}. Timeout increased to {3}",
                        _fileChunk, _count, _replicationDegree, timeout);
            }

            Core.Instance.Log.InfoFormat("{0}: Stored or giving up: retries {1}, rep degree {2}", _fileChunk, retryCount, _count);
            Core.Instance.Store.UpdateDegrees(_fileChunk.FileName, _count, _replicationDegree);
        }

        public Task Run()
        {
            Core.Instance.Log.InfoFormat("Starting BackupChunkSubprotocol: {0}, {1}", _fileChunk, _replicationDegree);
            return Task.Factory.StartNew(() =>
            {
                var subs = Core.Instance.MCChannel.Received
                    .Where(message => message.MessageType == MessageType.Stored)
                    .Cast<StoredMessage>()
                    .Where(message => message.ChunkNo == _fileChunk.ChunkNo &&
                        message.FileId == _fileChunk.FileId)
                    .Subscribe(StoreHandler);

                //Task.Factory.StartNew(SendChunk).ContinueWith(task => subs.Dispose());
                SendChunk();
                subs.Dispose(); // unsubscribe
            });
        }
    }
}

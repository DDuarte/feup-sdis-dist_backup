using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DBS.Messages;
using DBS.Messages.Enhancements;

namespace DBS.Protocols.Enhancements
{
    class EnhancedRestoreChunkProtocol : IProtocol
    {
        private readonly FileChunk _fileChunk;
        private const int Timeout = 5000;
        public ChunkMessage Message { get; private set; }
        public EnhancedRestoreChunkProtocol(FileChunk fileChunk)
        {
            _fileChunk = fileChunk;
        }

        public Task Run()
        {
            Core.Instance.Log.InfoFormat("Starting EnhancedRestoreChunkProtocol: {0}", _fileChunk);
            return Task.Factory.StartNew(() =>
            {
                Core.Instance.MCChannel.Send(new GetChunkMessage(_fileChunk));

                var src1 = Core.Instance.MDRChannel.Received
                    .Where(message => message.MessageType == MessageType.Chunk)
                    .Cast<ChunkMessage>()
                    .Where(message => message.ChunkNo == _fileChunk.ChunkNo &&
                                      message.FileId == _fileChunk.FileId)
                    .Cast<Message>();

                var src2 = Core.Instance.MDRChannel.Received
                    .Where(message => message.MessageType == MessageType.ACK)
                    .Cast<ACKMessage>()
                    .Where(message => message.ChunkNo == _fileChunk.ChunkNo &&
                                      message.FileId == _fileChunk.FileId)
                    .Cast<Message>();

                Message msg;
                try
                {
                    // wait for response
                    msg = src1.Merge(src2).Timeout(TimeSpan.FromMilliseconds(Timeout))
                        .Next().First();
                }
                catch (TimeoutException)
                {
                    Core.Instance.Log.ErrorFormat("EnhancedRestoreChunkProtocol: Could not fetch {0} from the network (timeout).", _fileChunk);
                    return;
                }

                if (msg.MessageType == MessageType.Chunk) // same behaviour as the regular protocol
                {
                    Message = msg as ChunkMessage;
                    return;
                }

                var ackMessage = msg as ACKMessage;
                if (ackMessage == null)
                {
                    Core.Instance.Log.ErrorFormat("EnhancedRestoreChunkProtocol: could not cast message {0} to ACK", msg);
                    return;
                }

                var listener = new TcpListener(IPAddress.Any, 0);
                listener.Start();

                Core.Instance.MCChannel.Send(new ConnInfoMessage(_fileChunk, ((IPEndPoint) listener.LocalEndpoint).Port,
                    ackMessage.RemoteEndPoint.Address));
                
                var clientTask = listener.AcceptTcpClientAsync();
                if (!clientTask.Wait(Timeout))
                {
                    Core.Instance.Log.Error("EnhancedRestoreChunkProtocol: listener.AcceptTcpClientAsync timed out");
                    return;
                }

                Core.Instance.Log.Info("EnhancedRestoreChunkProtocol: TcpClient accepted");
                try
                {
                    var stream = clientTask.Result.GetStream();
                    var bytes = new byte[Core.Instance.Config.ChunkSize];
                    stream.Read(bytes, 0, bytes.Length);
                    Message = new ChunkMessage(_fileChunk, bytes);
                    clientTask.Result.Close();
                }
                catch (Exception)
                {
                    Core.Instance.Log.Error("EnhancedRestoreChunkProtocol: error receiving chunk");
                }

                Core.Instance.Log.Info("EnhancedRestoreChunkProtocol: chunk was received with success");
            });
        }
    }
}  

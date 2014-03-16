using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;
using DBS.Multicast;

namespace DBS
{
    public interface IChannel
    {
        void Send(Message msg);
    }

    public delegate bool OnReceive(Message msg);

    public class Channel : IChannel
    {
        public string Name { get; set; }

        public readonly ConcurrentQueue<Message> Messages;

        public event OnReceive OnReceive;

        private void StartReceiving()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var msg = Receive();
                    if (OnReceive != null)
                    {
                        if (!OnReceive(msg))
                            Messages.Enqueue(msg);
                    }
                    else
                        Messages.Enqueue(msg);
                }
            });
        }

        private readonly IMulticastListener _listener;
        private readonly IMulticastBroadcaster _broadcaster;

        public Channel(IPAddress ip, int port)
        {
            var settings = new MulticastSettings {Address = ip, Port = port, TimeToLive = 3};

            _listener = new MulticastListener(settings);
            _broadcaster = new MulticastBroadcaster(settings);

            Messages = new ConcurrentQueue<Message>();
            StartReceiving();
        }

        public void Send(Message msg)
        {
            _broadcaster.Broadcast(msg.Serialize());
            //Console.WriteLine("S: " + msg);
        }

        private Message Receive()
        {
            return Message.Deserialize(_listener.Receive());
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;
using DBS.Multicast;

namespace DBS
{
    public delegate bool OnReceive(Message msg);

    public interface IChannel
    {
        event OnReceive OnReceive;

        string Name { get; set; }
        ConcurrentQueue<Message> Messages { get; set; }

        void Send(Message msg);
    }

    public class Channel : IChannel
    {
        public event OnReceive OnReceive;
        public string Name { get; set; }
        public ConcurrentQueue<Message> Messages { get; set; }

        private void StartReceiving()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var msg = Receive();
                    Console.WriteLine("R -  {0}: {1}", Name, msg);
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
            Console.WriteLine("S -  {0}: {1}", Name, msg);
        }

        private Message Receive()
        {
            IPEndPoint ep;
            var msg = Message.Deserialize(_listener.Receive(out ep));
            msg.RemoteEndPoint = ep;
            return msg;
        }
    }
}

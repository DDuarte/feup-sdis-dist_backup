using System;
using System.Net;
using System.Net.Sockets;
using DBS.Multicast;

namespace DBS
{
    public interface IChannel
    {
        //void JoinMulticast();
        //void DropMulticast();
        void Send(Message msg);
        Message Receive();
    }

    public class Channel : IChannel
    {
        public string Name { get; set; }

        private IMulticastListener Listener;
        private IMulticastBroadcaster Broadcaster;

        public Channel(IPAddress ip, int port)
        {
            var settings = new MulticastSettings {Address = ip, Port = port, TimeToLive = 3};

            Listener = new MulticastListener(settings, true);
            Broadcaster = new MulticastBroadcaster(settings, true);

            Listener.StartListening(data => Console.WriteLine(Message.Deserialize(data)));
        }

        public void Send(Message msg)
        {
            Broadcaster.Broadcast(msg.Serialize());
        }

        public Message Receive()
        {
            return new Message();
        }
    }
}

using System.Net;
using System.Net.Sockets;

namespace DBS
{
    public interface IChannel
    {
        void JoinMulticast();
        void DropMulticast();
        void Send(Message msg);
        Message Receive();
    }

    public class Channel : IChannel
    {
        public string Name { get; set; }

        private readonly IPAddress _ip; 
        private readonly UdpClient _udpClient;
        private IPEndPoint _remotePoint;

        public Channel(IPAddress ip, int port)
        {
            _udpClient = new UdpClient {Ttl = 1};
            _ip = ip;
            _remotePoint = new IPEndPoint(_ip, port);
        }

        public void JoinMulticast()
        {
            _udpClient.JoinMulticastGroup(_ip);
        }

        public void DropMulticast()
        {
            _udpClient.DropMulticastGroup(_ip);
        }

        public void Send(Message msg)
        {
            var bytes = msg.Serialize();
            _udpClient.Send(bytes, bytes.Length, _remotePoint);
        }

        public Message Receive()
        {
            var data = _udpClient.Receive(ref _remotePoint);
            return Message.Deserialize(data);
        }
    }
}

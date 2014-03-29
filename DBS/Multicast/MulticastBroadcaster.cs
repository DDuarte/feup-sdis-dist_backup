using System;
using System.Net;
using System.Net.Sockets;

namespace DBS.Multicast
{
    class MulticastBroadcaster
    {
        public MulticastSettings Settings { get; private set; }

        public bool IsBound
        {
            get
            {
                return UdpClient.Client != null && UdpClient.Client.IsBound;
            }
        }

        public IPEndPoint LocalIPEndPoint { get; protected set; }
        public IPEndPoint RemoteIPEndPoint { get; protected set; }

        private UdpClient _udpClient;
        public UdpClient UdpClient
        {
            get { return _udpClient ?? (_udpClient = new UdpClient()); }
        }

        public MulticastBroadcaster(MulticastSettings settings, bool autoBindJoinConnect = true)
        {
            if (settings == null) throw new ArgumentNullException("settings");

            Settings = settings;

            if (autoBindJoinConnect) BindJoinConnect();
        }

        private void BindJoinConnect()
        {
            LocalIPEndPoint = new IPEndPoint(Core.Instance.Config.LocalIP, Settings.Port);
            RemoteIPEndPoint = new IPEndPoint(Settings.Address, Settings.Port);

            UdpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            UdpClient.ExclusiveAddressUse = false;
            UdpClient.EnableBroadcast = true;
            UdpClient.MulticastLoopback = true;

            UdpClient.Client.Bind(LocalIPEndPoint);
            UdpClient.JoinMulticastGroup(Settings.Address, Settings.TimeToLive);
            UdpClient.Connect(RemoteIPEndPoint);
        }

        private void UnbindLeaveDisconnect()
        {
            UdpClient.DropMulticastGroup(Settings.Address);
            UdpClient.Close();
        }

        public void Broadcast(byte[] data)
        {
            if (!IsBound) BindJoinConnect();
            UdpClient.Send(data, data.Length);
        }

        public void Dispose()
        {
            UnbindLeaveDisconnect();
        }
    }
}

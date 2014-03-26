using System;
using System.Net;
using System.Net.Sockets;

namespace DBS.Multicast
{
    class MulticastBroadcaster : IMulticastBroadcaster
    {
        public MulticastSettings Settings { get; protected set; }

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
            UdpClient.MulticastLoopback = false;

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

            var broadcastCallback = new AsyncCallback(BroadcastCallback);
            UdpClient.BeginSend(data, data.Length, broadcastCallback, this);
        }

        private void BroadcastCallback(IAsyncResult ar)
        {
            try
            {
                var broadcaster = (MulticastBroadcaster)(ar.AsyncState);

                var udpClient = broadcaster.UdpClient;
                udpClient.EndSend(ar);
            }
            catch (ObjectDisposedException)
            {
                // expected exception fired when the socket is closed - swallow it up
            }
        }

        public void Dispose()
        {
            UnbindLeaveDisconnect();
        }
    }
}

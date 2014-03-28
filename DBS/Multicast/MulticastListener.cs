using System;
using System.Net;
using System.Net.Sockets;

namespace DBS.Multicast
{
    class MulticastListener : IMulticastListener
    {
        public MulticastSettings Settings { get; private set; }

        public bool IsBound
        {
            get
            {
                return UdpClient.Client != null && UdpClient.Client.IsBound;
            }
        }

        private UdpClient _udpClient;
        public UdpClient UdpClient
        {
            get { return _udpClient ?? (_udpClient = new UdpClient()); }
        }

        public IPEndPoint LocalIPEndPoint { get; protected set; }

        public MulticastListener(MulticastSettings settings, bool autoBindJoinConnect = true)
        {
            if (settings == null) throw new ArgumentNullException("settings");

            Settings = settings;

            if (autoBindJoinConnect) BindAndJoin();
        }

        private void BindAndJoin()
        {
            LocalIPEndPoint = new IPEndPoint(Core.Instance.Config.LocalIP, Settings.Port);
            UdpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            UdpClient.ExclusiveAddressUse = false;
            UdpClient.EnableBroadcast = true;
            UdpClient.MulticastLoopback = true;

            UdpClient.Client.Bind(LocalIPEndPoint);
            UdpClient.JoinMulticastGroup(Settings.Address, Settings.TimeToLive);
        }

        public byte[] Receive(out IPEndPoint ep)
        {
            var ipEndPoint = LocalIPEndPoint;

            try
            {
                var msg = UdpClient.Receive(ref ipEndPoint);
                ep = ipEndPoint;
                return msg;
            }
            catch (SocketException)
            {
                ep = null;
                return null;
            }
        }

        public byte[] Receive()
        {
            IPEndPoint _;
            return Receive(out _);
        }

        public void StopListening()
        {
            if (IsBound) UnbindAndLeave();
        }

        private void UnbindAndLeave()
        {
            try
            {
                UdpClient.DropMulticastGroup(Settings.Address);
                UdpClient.Close();
            }
            catch (ObjectDisposedException)
            {
                // expected exception fired when we close - swallow it up
            }
        }

        public void Dispose()
        {
            if (IsBound) UnbindAndLeave();
        }
    }
}

using System;
using System.Net;
using System.Net.Sockets;
using JsonConfig;

namespace DBS.Multicast
{
    class MulticastListener : IMulticastListener
    {
        public event ReceiveHandler OnReceive;

        public MulticastSettings Settings { get; protected set; }

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
            LocalIPEndPoint = new IPEndPoint(IPAddress.Parse(Config.Global.LocalIP), Settings.Port);
            UdpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            UdpClient.ExclusiveAddressUse = false;
            UdpClient.EnableBroadcast = true;

            UdpClient.Client.Bind(LocalIPEndPoint);
            UdpClient.JoinMulticastGroup(Settings.Address, Settings.TimeToLive);
        }

        public void StartListening(ReceiveHandler handler)
        {
            if (!IsBound) BindAndJoin();

            OnReceive += handler;
            var receiveCallback = new AsyncCallback(ReceiveCallback);
            UdpClient.BeginReceive(receiveCallback, this);
        }

        public void StopListening()
        {
            OnReceive = null;
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

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                var receiver = (MulticastListener)(ar.AsyncState);

                var udpClient = receiver.UdpClient;
                var ipEndPoint = receiver.LocalIPEndPoint;

                var receiveBytes = udpClient.EndReceive(ar, ref ipEndPoint);
                OnReceive(receiveBytes);

                var receiveCallback = new AsyncCallback(ReceiveCallback);
                UdpClient.BeginReceive(receiveCallback, this);
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

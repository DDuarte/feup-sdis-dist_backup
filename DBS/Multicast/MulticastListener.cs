using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using DBS.Utilities;

namespace DBS.Multicast
{
    class MulticastListener
    {
        private readonly Subject<Tuple<byte[], IPEndPoint>> _subj = new Subject<Tuple<byte[], IPEndPoint>>();
        public IObservable<Tuple<byte[], IPEndPoint>> Received { get { return _subj.AsObservable(); } }
        private readonly Task _receiveTask;
        private readonly Task _onNextingTask;

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

            _receiveTask = new Task(StartReceiving, TaskCreationOptions.LongRunning);
            _receiveTask.Start();

            _onNextingTask = new Task(StartOnNexting, TaskCreationOptions.LongRunning);
            _onNextingTask.Start();
        }

        //private ConcurrentQueue<Tuple<byte[], IPEndPoint>> _queue = new ConcurrentQueue<Tuple<byte[], IPEndPoint>>();
        private readonly BlockingCollection<Tuple<byte[], IPEndPoint>> _queue = new BlockingCollection<Tuple<byte[], IPEndPoint>>();

        private void StartReceiving()
        {
            while (true)
            {
                try
                {
                    var ipEndPoint = LocalIPEndPoint;
                    var data = UdpClient.Receive(ref ipEndPoint);
                    _queue.Add(Tuple.Create(data, ipEndPoint));
                }
                catch (Exception ex)
                {
                    Core.Instance.Log.Error("Failed to receive on {0}:{1}".FormatWith(Settings.Address, Settings.Port), ex);
                }
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private void StartOnNexting()
        {
            while (true)
            {
                _subj.OnNext(_queue.Take());
            }
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
            _subj.OnCompleted();
            _subj.Dispose();
            _receiveTask.Dispose();
            _onNextingTask.Dispose();
            if (IsBound) UnbindAndLeave();
        }
    }
}

using System;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DBS.Messages;
using DBS.Multicast;

namespace DBS
{
    public class Channel : IDisposable
    {
        public string Name { get; set; }

        private readonly IMulticastListener _listener;
        private readonly IMulticastBroadcaster _broadcaster;

        private readonly Subject<Message> _subject;

        public IObservable<Message> Received { get { return _subject.AsObservable(); } } 

        public Channel(IPAddress ip, int port)
        {
            var settings = new MulticastSettings {Address = ip, Port = port, TimeToLive = 3};

            _listener = new MulticastListener(settings);
            _broadcaster = new MulticastBroadcaster(settings);

            _subject = new Subject<Message>();

            ((MulticastListener) _listener).Received.Subscribe(OnReceive);
        }

        private void OnReceive(Tuple<byte[], IPEndPoint> tuple)
        {
            try
            {
                var msg = Message.Deserialize(tuple.Item1);
                msg.RemoteEndPoint = tuple.Item2;
                Core.Instance.Log.CustomFormat("receive", "R[{0}]: {1} from {2}", Name.PadLeft(3), msg, msg.RemoteEndPoint);
                _subject.OnNext(msg);
            }
            catch (Exception ex)
            {
                Core.Instance.Log.Error("Failed on deserialization", ex);
            }
        }

        public void Send(Message msg)
        {
            _broadcaster.Broadcast(msg.Serialize());
            Core.Instance.Log.CustomFormat("send", "S[{0}]: {1}", Name.PadLeft(3), msg);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            _broadcaster.Dispose();
            _listener.Dispose();

            _subject.OnCompleted();
            _subject.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}

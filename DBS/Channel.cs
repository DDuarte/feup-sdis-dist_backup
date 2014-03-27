﻿using System;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using DBS.Messages;
using DBS.Multicast;
using DBS.Utilities;

namespace DBS
{
    public class Channel : IDisposable
    {
        public string Name { get; set; }

        private void StartReceiving()
        {
            while (true)
            {
                Message msg;
                try
                {
                    msg = Receive();
                }
                catch (Exception ex)
                {
                    Core.Instance.Log.Error("Failed to receive a message on channel {0}".FormatWith(Name), ex);
                    continue;
                }

                Core.Instance.Log.CustomFormat("receive", "R[{0}]: {1} from {2}", Name.PadLeft(3), msg, msg.RemoteEndPoint);
                _subject.OnNext(msg);
            }
// ReSharper disable once FunctionNeverReturns
        }

        private readonly IMulticastListener _listener;
        private readonly IMulticastBroadcaster _broadcaster;
        private readonly Task _receiveTask;

        private readonly Subject<Message> _subject;

        public IObservable<Message> Received { get { return _subject.AsObservable(); } } 

        public Channel(IPAddress ip, int port)
        {
            var settings = new MulticastSettings {Address = ip, Port = port, TimeToLive = 3};

            _listener = new MulticastListener(settings);
            _broadcaster = new MulticastBroadcaster(settings);

            _subject = new Subject<Message>();

            _receiveTask = new Task(StartReceiving, TaskCreationOptions.LongRunning);
            _receiveTask.Start();
        }

        public void Send(Message msg)
        {
            _broadcaster.Broadcast(msg.Serialize());
            Core.Instance.Log.CustomFormat("send", "S[{0}]: {1}", Name.PadLeft(3), msg);
        }

        private Message Receive()
        {
            IPEndPoint ep;
            var msg = Message.Deserialize(_listener.Receive(out ep));
            msg.RemoteEndPoint = ep;
            return msg;
        }

        public void Dispose()
        {
            _receiveTask.Dispose();
            _broadcaster.Dispose();
            _listener.Dispose();

            _subject.OnCompleted();
            _subject.Dispose();
        }
    }
}

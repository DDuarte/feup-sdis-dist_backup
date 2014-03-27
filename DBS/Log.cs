using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using DBS.Utilities;

namespace DBS
{
    public interface ILog : IObservable<string>, IDisposable
    {
        void Error(string msg, Exception ex);
        void ErrorFormat(string format, params object[] args);
        void Error(string msg);
        void Info(string msg, Exception ex);
        void InfoFormat(string format, params object[] args);
        void Info(string msg);
        void Custom(string t, string msg);
        void CustomFormat(string t, string format, params object[] args);
    }

    public class Log : ILog
    {
        private readonly Subject<string> _infoSubj = new Subject<string>();
        private readonly Subject<string> _errorSubj = new Subject<string>();
        private readonly Dictionary<string, Subject<string>> _customSubj = new Dictionary<string, Subject<string>>();
        private IObservable<string> ErrorLog { get { return _errorSubj; } }
        private IObservable<string> InfoLog { get { return _infoSubj; } }

        private Dictionary<string, IObservable<string>> CustomLog
        {
            get
            {
                return _customSubj.ToDictionary<KeyValuePair<string, Subject<string>>, string, IObservable<string>>(
                    pair => pair.Key, pair => pair.Value);
            }
        }

        public void Error(string msg, Exception ex)
        {
            Error(msg + " - " + ex);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            Error(format.FormatWith(args));
        }

        public void Error(string msg)
        {
            var date = DateTime.UtcNow.ToShortTimeString();
            var log = "E" + date + ": " + msg;
            _errorSubj.OnNext(log);
        }

        public void Info(string msg, Exception ex)
        {
            Error(msg + " - " + ex);
        }

        public void InfoFormat(string format, params object[] args)
        {
            Info(format.FormatWith(args));
        }

        public void Info(string msg)
        {
            var date = DateTime.UtcNow.ToLongTimeString();
            var log = "I" + date + ": " + msg;
            _infoSubj.OnNext(log);
        }

        public void Custom(string t, string msg)
        {
            var date = DateTime.UtcNow.ToLongTimeString();
            var log = "C" + date + ": " + msg;
            _customSubj.GetOrCreate(t).OnNext(log);
        }

        public void CustomFormat(string t, string format, params object[] args)
        {
            Custom(t, format.FormatWith(args));
        }

        public IDisposable Subscribe(IObserver<string> observer)
        {
            var allLogs = ErrorLog.Merge(InfoLog);
            foreach (var obs in CustomLog)
                allLogs.Merge(obs.Value);
            return allLogs.Subscribe(observer);
        }

        public IDisposable SubscribeOn(IObserver<string> observer, SynchronizationContext context)
        {
            var allLogs = ErrorLog.Merge(InfoLog);
            allLogs = CustomLog.Aggregate(allLogs, (current, obs) => current.Merge(obs.Value));
            return allLogs.ObserveOn(context).Subscribe(observer);
        }

        public IDisposable SubscribeError(IObserver<string> observer)
        {
            return ErrorLog.Subscribe(observer);
        }

        public IDisposable SubscribeErrorOn(IObserver<string> observer, SynchronizationContext context)
        {
            return ErrorLog.ObserveOn(context).Subscribe(observer);
        }

        public IDisposable SubscribeInfo(IObserver<string> observer)
        {
            return InfoLog.Subscribe(observer);
        }

        public IDisposable SubscribeInfoOn(IObserver<string> observer, SynchronizationContext context)
        {
            return InfoLog.ObserveOn(context).Subscribe(observer);
        }

        public IDisposable SubscribeCustom(string t, IObserver<string> observer)
        {
            IObservable<string> observ;
            if (!CustomLog.TryGetValue(t, out observ))
            {
                var subj = new Subject<string>();
                _customSubj.Add(t, subj);
                observ = subj;
            }

            return observ.Subscribe(observer);
        }

        public IDisposable SubscribeCustomOn(string t, IObserver<string> observer, SynchronizationContext context)
        {
            IObservable<string> observ;
            if (!CustomLog.TryGetValue(t, out observ))
            {
                var subj = new Subject<string>();
                _customSubj.Add(t, subj);
                observ = subj;
            }

            return observ.ObserveOn(context).Subscribe(observer);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            _infoSubj.Dispose();
            _errorSubj.Dispose();
            foreach (var s in _customSubj)
                s.Value.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}

using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DBS.Utilities;

namespace DBS
{
    public interface ILog : IObservable<string>
    {
        void Error(string msg, Exception ex);
        void ErrorFormat(string format, params object[] args);
        void Error(string msg);
        void Info(string msg, Exception ex);
        void InfoFormat(string format, params object[] args);
        void Info(string msg);
    }

    public class Log : ILog
    {
        private readonly Subject<string> _infoSubj = new Subject<string>();
        private readonly Subject<string> _errorSubj = new Subject<string>();
        private IObservable<string> _errorLog { get { return _errorSubj.AsObservable(); } }
        private IObservable<string> _infoLog { get { return _infoSubj.AsObservable(); } }
        private IObservable<string> _logs { get { return _infoLog.Merge(_errorLog); } } 

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
            var date = DateTime.UtcNow.ToShortTimeString();
            var log = "I" + date + ": " + msg;
            _infoSubj.OnNext(log);
        }

        public IDisposable Subscribe(IObserver<string> observer)
        {
            return _logs.Subscribe(observer);
        }

        public IDisposable SubscribeError(IObserver<string> observer)
        {
            return _errorLog.Subscribe(observer);
        }

        public IDisposable SubscribeInfo(IObserver<string> observer)
        {
            return _infoLog.Subscribe(observer);
        }
    }
}

using System;

namespace DBS
{
    public interface ILog : IDisposable
    {
        void Error(string msg, Exception ex);
        void ErrorFormat(string format, params object[] args);
        void Error(string msg);
        void Info(string msg, Exception ex);
        void InfoFormat(string format, params object[] args);
        void Info(string msg);
        void Custom(string t, string msg, Exception ex);
        void CustomFormat(string t, string format, params object[] args);
        void Custom(string t, string msg);
    }
}

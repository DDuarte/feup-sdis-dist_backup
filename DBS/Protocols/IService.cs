using System;

namespace DBS.Protocols
{
    /// <summary>
    /// Passive peers
    /// </summary>
    interface IService : IObserver<Message>
    {
        void Start();
        void Stop(); // maybe
    }
}

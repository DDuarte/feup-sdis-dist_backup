using System;

namespace DBS.Protocols
{
    /// <summary>
    /// Passive peers
    /// </summary>
    interface IService<in T> : IObserver<T> where T : Message
    {
        void Start();
        void Stop(); // maybe
    }
}

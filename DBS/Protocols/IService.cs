using System;
using DBS.Messages;

namespace DBS.Protocols
{
    /// <summary>
    /// Passive peers
    /// </summary>
    public interface IService
    {
        void Start();
        void Stop(); // maybe
    }

    // force implementations of IObserver<Message>
    public interface IServiceObserver<in T> : IService, IObserver<T> where T : Message
    {
        
    }
}

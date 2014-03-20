using System;

namespace DBS
{
    interface IService : IObserver<Message>
    {
        void Start();
        void Stop(); // maybe
    }
}

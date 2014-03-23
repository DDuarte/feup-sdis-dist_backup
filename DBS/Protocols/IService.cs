using System;

namespace DBS.Protocols
{
    interface IService : IObserver<Message>
    {
        void Start();
        void Stop(); // maybe
    }

    // TODO: SpaceReclaimingService (in Program.cs)
}

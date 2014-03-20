using System;

namespace DBS
{
    interface IService : IObserver<Message>
    {
        void Start();
        void Stop(); // maybe
    }

    // TODO: SpaceReclaimingService (in Program.cs)
    // TODO: ChunkRestoreService
    // TODO: ChunkRestoreProtocol
}

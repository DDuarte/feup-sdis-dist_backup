using System;

namespace DBS.Multicast
{
    public interface IMulticastListener : IDisposable
    {
        MulticastSettings Settings { get; }

        bool IsBound { get; }

        byte[] Receive();

        void StopListening();
    }
}

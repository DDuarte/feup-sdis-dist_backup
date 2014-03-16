using System;
using System.Net;

namespace DBS.Multicast
{
    public interface IMulticastListener : IDisposable
    {
        MulticastSettings Settings { get; }

        bool IsBound { get; }

        byte[] Receive();
        byte[] Receive(out IPEndPoint ep);

        void StopListening();
    }
}

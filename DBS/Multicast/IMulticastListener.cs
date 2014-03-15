using System;

namespace DBS.Multicast
{
    public delegate void ReceiveHandler(byte[] data);

    public interface IMulticastListener : IDisposable
    {
        event ReceiveHandler OnReceive;

        MulticastSettings Settings { get; }

        bool IsBound { get; }

        byte[] Receive();

        void StopListening();
    }
}

﻿using System;

namespace DBS.Multicast
{
    public interface IMulticastBroadcaster : IDisposable
    {
        MulticastSettings Settings { get; }

        bool IsBound { get; }

        void Broadcast(byte[] data);
    }
}

namespace DBS.Multicast
{
    public interface IMulticastBroadcaster
    {
        MulticastSettings Settings { get; }

        bool IsBound { get; }

        void Broadcast(byte[] data);
    }
}

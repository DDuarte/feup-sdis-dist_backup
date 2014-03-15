using System.Net;

namespace DBS.Multicast
{
    public class MulticastSettings
    {
        public IPAddress Address { get; set; }
        public int Port { get; set; }
        public int TimeToLive { get; set; }
    }
}

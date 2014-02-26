using System.Linq;
using System.Net;
using System.Net.Sockets;
using DBS.Annotations;

namespace DBS
{
    [UsedImplicitly]
    public class NetworkUtilities
    {
        /// <summary>
        /// Returns all IPv4 local IP addresses
        /// </summary>
        /// <returns>Array of addresses</returns>
        [UsedImplicitly]
        public static IPAddress[] GetLocalIPAddresses()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            return host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToArray();
        }
    }
}

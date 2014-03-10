using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
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

        /// <summary>
        /// Returns identifier of the first network adapter or empty string, if no interface is found
        /// </summary>
        /// <returns>Identifier of a network adapter</returns>
        public static string GetNetworkId()
        {
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            return networkInterfaces.Length > 0 ? networkInterfaces.First().Id : "";
        }
    }
}

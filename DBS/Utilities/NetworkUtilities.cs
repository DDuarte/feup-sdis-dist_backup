using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace DBS.Utilities
{
    public static class NetworkUtilities
    {
        /// <summary>
        /// Returns all IPv4 local IP addresses
        /// </summary>
        /// <returns>Array of addresses</returns>
        public static IEnumerable<IPAddress> GetLocalIPAddresses()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            return host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static bool IsMulticastAddress(IPAddress address)
        {
            return address.IsIPv6Multicast  || (address.GetAddressBytes()[0] & 0xF0) == 224;
        }
    }
}

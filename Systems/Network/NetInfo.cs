using System.Net;
using System.Net.Sockets;

namespace BazthalLib.Systems.Network
{
    public class NetInfo
    {
        /// <summary>
        /// Retrieves the local IP address of the machine.
        /// </summary>
        /// <remarks>This method returns the first IPv4 address found in the machine's network interfaces.
        /// If no IPv4 address is found, it returns "Not Found".</remarks>
        /// <returns>A string representing the local IPv4 address. Returns "Not Found" if no IPv4 address is available.</returns>
        public static string GetLocalIPAddress()
        {
            string ipAddress = "Not Found";
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAddress = ip.ToString();
                    break;
                }
            }

            return ipAddress;
        }
    }
}

using SAPLib.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace SAPLib
{
    public class SAPService : IDisposable
    {

        public event EventHandler<MessageSendEventArgs> OnMessageSend;

        System.Timers.Timer _timer = new System.Timers.Timer();
        UdpClient _udpClient = new UdpClient();

        public IPAddress Address { get; set; } = IPAddress.Parse("239.255.255.255");
        public int Port { get; set; } = 9875;

        public Dictionary<string, SapPacket> Program { get; set; } = new Dictionary<string, SapPacket>();

        public TimeSpan Interval
        {
            get
            {
                return TimeSpan.FromMilliseconds(_timer.Interval);
            }
            set
            {
                _timer.Interval = value.TotalMilliseconds;
            }
        }

        /// <summary>
        /// Simple constructor for the
        /// </summary>
        public SAPService()
        {
            this._udpClient.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 255);
            this.Interval = TimeSpan.FromSeconds(5);
            this._timer.AutoReset = true;
            this._timer.Enabled = true;
            this._timer.Elapsed += OnTimedEvent;
        }

        private async void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            foreach (KeyValuePair<string, SapPacket> item in this.Program)
            {
                byte[] buffer = item.Value.ToBytes();
                await _udpClient.SendAsync(buffer, buffer.Length, new IPEndPoint(this.Address, this.Port));

                OnMessageSend?.Invoke(this, 
                    new MessageSendEventArgs() 
                    { 
                        Packet = item.Value
                    });
            }
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        /// <summary>
        /// This method sets the outgoing interface when a socket sends data to a multicast
        /// group. Because multicast addresses are not routable, the network stack simply
        /// picks the first interface in the routing table with a multicast route. In order
        /// to change this behavior, the MulticastInterface option can be used to set the
        /// local interface on which all outgoing multicast traffic is to be sent (for this
        /// socket only). This is done by converting the 4 byte IPv4 address (or 16 byte
        /// IPv6 address) into a byte array.
        /// </summary>
        /// <param name="sendInterface"></param>
        public void SetSendInterface(IPAddress sendInterface)
        {
            // Set the outgoing multicast interface
            try
            {
                Console.WriteLine("Setting the outgoing multicast interface...");
                if (this._udpClient.Client.AddressFamily == AddressFamily.InterNetwork)
                {
                    this._udpClient.Client.SetSocketOption(
                        SocketOptionLevel.IP,
                        SocketOptionName.MulticastInterface,
                        sendInterface.GetAddressBytes()
                    );
                }
                else
                {
                    byte[] interfaceArray = BitConverter.GetBytes((int)sendInterface.ScopeId);
                    this._udpClient.Client.SetSocketOption(
                        SocketOptionLevel.IPv6,
                        SocketOptionName.MulticastInterface,
                        interfaceArray
                    );
                }
                Console.WriteLine("Setting multicast send interface to: " + sendInterface.ToString());
            }
            catch (SocketException err)
            {
                Console.WriteLine("SetSendInterface: Unable to set the multicast interface: {0}", err.Message);
                throw;

            }
        }

        public void Dispose()
        {
            this._timer.Dispose();
            this._udpClient.Dispose();
        }
        public static IPAddress GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }



        public static IList<IPAddress> GetAllIPAddress()
        {
            List<IPAddress> result = new List<IPAddress>();

            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in nics)
            {
                IPInterfaceProperties ip_properties = adapter.GetIPProperties();
                if (!adapter.GetIPProperties().MulticastAddresses.Any())
                    continue; // most of VPN adapters will be skipped
                if (!adapter.SupportsMulticast)
                    continue; // multicast is meaningless for this type of connection
                if (OperationalStatus.Up != adapter.OperationalStatus)
                    continue; // this adapter is off or not connected
                IPv4InterfaceProperties p = adapter.GetIPProperties().GetIPv4Properties();
                if (null == p)
                    continue; // IPv4 is not configured on this adapter
            }
            return result;
        }

    }
}

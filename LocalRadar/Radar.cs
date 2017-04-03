using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace LocalRadar
{
    public class IPRange
    {
        IPAddress start;
        IPAddress end;

        public IPRange(IPAddress start, IPAddress end)
        {
            this.start = start;
            this.end = end;
        }

        public IEnumerable<IPAddress> GetIPRange()
        {
            uint sIP = ipToUint(start.GetAddressBytes());
            uint eIP = ipToUint(end.GetAddressBytes());
            while (sIP <= eIP)
            {
                yield return new IPAddress(reverseBytesArray(sIP));
                sIP++;
            }
        }
        protected uint reverseBytesArray(uint ip)
        {
            byte[] bytes = BitConverter.GetBytes(ip);
            Array.Reverse(bytes);
            return (uint)BitConverter.ToInt32(bytes, 0);
        }


        /* Convert bytes array to 32 bit long value */
        protected uint ipToUint(byte[] ipBytes)
        {
            ByteConverter bConvert = new ByteConverter();
            uint ipUint = 0;

            int shift = 24; // indicates number of bits left for shifting
            foreach (byte b in ipBytes)
            {
                if (ipUint == 0)
                {
                    ipUint = (uint)bConvert.ConvertTo(b, typeof(uint)) << shift;
                    shift -= 8;
                    continue;
                }

                if (shift >= 8)
                    ipUint += (uint)bConvert.ConvertTo(b, typeof(uint)) << shift;
                else
                    ipUint += (uint)bConvert.ConvertTo(b, typeof(uint));

                shift -= 8;
            }

            return ipUint;
        }
    }

    public class Radar
    {
        private IPRange range;
        private int port;

        private Thread scanningThread;

        private Action<IPAddress> findCallback; 
        private Action endCallback;

        internal void SetRange(IPRange range) { this.range = range; }
        internal void SetPort(int port) { this.port = port; }
        internal void SetFindCallback(Action<IPAddress> callback) { findCallback = callback; }
        internal void SetEndCallback(Action callback) { endCallback = callback; }

        public void Scan()
        {
            scanningThread = new Thread(Scanning);
            scanningThread.Start();
        }

        public void Stop() { scanningThread.Abort(); scanningThread.Join(500); }

        private async void Scanning()
        {
            IPEndPoint localIp = null;

            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIp = new IPEndPoint(ip, 0);
                    break;
                }
            }

            Ping ping = new Ping();

            foreach (var ip in range.GetIPRange())
            {
                PingReply reply = await ping.SendPingAsync(ip, 100);
                if (reply != null && reply.Status == IPStatus.Success)
                    Console.WriteLine(reply.Address);
            }

            Console.WriteLine("End");
        }
    }

    public class RadarBuilder
    {
        private Radar radar = new Radar();

        public RadarBuilder SetRange(IPAddress start, IPAddress end)
        {
            radar.SetRange(new IPRange(start, end));
            return this;
        }

        public RadarBuilder SetPort(int port) {
            radar.SetPort(port);
            return this;
        }

        public RadarBuilder SetFindCallback(Action<IPAddress> callback)
        {
            radar.SetFindCallback(callback);
            return this;
        }

        public RadarBuilder SetEndCallback(Action callback)
        {
            radar.SetEndCallback(callback);
            return this;
        }

        public Radar Build() { return radar; }
    }
}

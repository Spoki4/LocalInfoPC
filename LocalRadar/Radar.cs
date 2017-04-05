using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace LocalRadar
{
    public class Radar
    {
        private IPRange range;
        private int port;

        private Thread scanningThread;
        private volatile bool threadRunning;

        private Action<IPAddress> findCallback; 
        private Action endCallback;

        private int frequencyTime = 5000;

        internal void SetRange(IPRange range) { this.range = range; }
        internal void SetPort(int port) { this.port = port; }
        internal void SetFindCallback(Action<IPAddress> callback) { findCallback = callback; }
        internal void SetEndCallback(Action callback) { endCallback = callback; }
        internal void SetFrenquency(int millis) { frequencyTime = millis; }

        public void Scan()
        {
            threadRunning = true;
            scanningThread = new Thread(Scanning);
            scanningThread.Start();
        }

        public void Stop() { threadRunning = false; }

        private void Scanning()
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

            PingCompletedEventHandler foundHandler = (object sender, PingCompletedEventArgs args) =>
            {
                if(args.Reply != null && args.Reply.Status == IPStatus.Success)
                {
                    Console.WriteLine("IP {0} found in local network", args.Reply.Address);
                    (args.UserState as Action<IPAddress>)(args.Reply.Address);
                }
            };

            while (threadRunning)
            {
                foreach (var ip in range.GetIPRange())
                {
                    if (!threadRunning)
                        return;
                    Ping ping = new Ping();
                    ping.PingCompleted += foundHandler;
                    ping.SendAsync(ip, 300, findCallback);
                }

                Thread.Sleep(frequencyTime);
            }
        }
    }

    
}

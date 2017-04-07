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

        private Action<IPAddress, Radar> findCallback; 
        private Action endCallback;

        private int frequencyTime = 5000;

        internal void SetRange(IPRange range) { this.range = range; }
        internal void SetPort(int port) { this.port = port; }
        internal void SetFindCallback(Action<IPAddress, Radar> callback) { findCallback = callback; }
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

            AsyncCallback foundHandler = (IAsyncResult args) =>
            {
                var client = args.AsyncState as TcpClient;
                client.EndConnect(args);

                Console.WriteLine("Good {0}",client.Client.RemoteEndPoint.ToString());
           
            };
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, false);

            while (threadRunning)
            {
                foreach (var ip in range.GetIPRange())
                {
                    if (!threadRunning)
                        return;
                    try {
                        IAsyncResult result = socket.BeginConnect(ip, port, null, null);
                        bool success = result.AsyncWaitHandle.WaitOne(200, true);
                        if (socket.Connected)
                            Console.WriteLine("ip {0} good", ip);
                    }
                    catch (Exception)
                    {

                    }
                }

                Thread.Sleep(frequencyTime);
            }
        }
    }

    
}

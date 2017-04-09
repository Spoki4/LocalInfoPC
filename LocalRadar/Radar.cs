using NetworkConstants;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LocalRadar
{
    public class Radar
    {
        private IPRange range;
        private int port;

        private Thread scanningThread;
        private volatile bool threadRunning;

        private Action<TcpClient> findCallback;
        private Action endCallback;

        private int frequencyTime = 5000;

        internal void SetRange(IPRange range) { this.range = range; }
        internal void SetPort(int port) { this.port = port; }
        internal void SetFindCallback(Action<TcpClient> callback) { findCallback = callback; }
        internal void SetEndCallback(Action callback) { endCallback = callback; }
        internal void SetFrenquency(int millis) { frequencyTime = millis; }

        public void Scan()
        {
            threadRunning = true;
            scanningThread = new Thread(Scanning);
            scanningThread.Start();
        }

        public void Stop() { threadRunning = false; }

        struct AsyncObject
        {
            public TcpClient connection;
            public byte[] receiveBuffer;
        }

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

            var receiveCallback = new AsyncCallback((IAsyncResult ar) =>
            {
                AsyncObject state = (AsyncObject)ar.AsyncState;
                Console.WriteLine("[{0}] Trying read data from stream");

                int readBytes = state.connection.Client.EndReceive(ar);
                
                if (readBytes > 0)
                {
                    Console.WriteLine("[{0}]Data length = {1} and string = {2}", state.connection.Client.RemoteEndPoint, readBytes, Encoding.ASCII.GetString(state.receiveBuffer));
                }
            });
            
            while (threadRunning)
            {
                foreach (var ip in range.GetIPRange())
                {
                    if (!threadRunning)
                        return;
                    try
                    {
                        TcpClient tester = new TcpClient();
                        AsyncObject state = new AsyncObject();
                        state.connection = tester;

                        tester.BeginConnect(ip, port, (IAsyncResult ar) => {
                            AsyncObject obj = (AsyncObject)ar.AsyncState;
                            try { 
                                obj.connection.EndConnect(ar);

                                if (obj.connection.Connected) {
                                    Console.WriteLine("Connected to {0}", obj.connection.Client.RemoteEndPoint);
                                    var buffer = Encoding.ASCII.GetBytes(Constants.NETWORK_PING);
                                    tester.Client.Send(buffer);

                                    state.receiveBuffer = new byte[buffer.Length];

                                    tester.Client.BeginReceive(state.receiveBuffer, 0, state.receiveBuffer.Length, 0, receiveCallback, state);
                                }
                            } catch(SocketException)
                            {

                            }
                        }, state);
                    }
                    catch (Exception)
                    {

                    }

                    Thread.Sleep(10);
                }

                Thread.Sleep(frequencyTime);
            }
        }
    }

    
}

using PCInfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        byte[] HelloHeader = { 0x11, 0x22, 0x33, 0x44, 0x55 };

        static void Main(string[] args)
        {
            bool isClientMode = true;
            foreach(var arg in args)
            {
                if(arg.ToLower().Equals("--help"))
                {
                    Console.WriteLine(new String('-', 80));
                    Console.WriteLine("This program used for get remote information about pc");
                    Console.WriteLine("\t--server -S use to start server instance");
                    Console.WriteLine(new String('-', 80));
                    return;
                }
                else if(arg.ToLower().Equals("--server") || arg.ToLower().Equals("-S"))
                {
                    isClientMode = false;
                }
            }

            Program prog = new Program();

            if (isClientMode)
                prog.workAsClient();
            else
                prog.workAsServer();
        }

        public void workAsClient()
        {
            TcpClient listener = new TcpClient(new IPEndPoint(IPAddress.Any, 3210));

            Console.WriteLine("Waiting for a server found this client...");

            Info info = new Info();
            var radar = new LocalRadar.RadarBuilder()
                .SetRange(IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.255.255"))
                .SetPort(3211)
                .SetFrequency(15000)
                .SetFindCallback((IPAddress address, LocalRadar.Radar r) =>
                {
                    try { 
                        listener.Connect(address, 3211);
                    } catch(Exception e)
                    {
                        Console.WriteLine(e);
                        return;
                    }

                    byte[] helloMsg = new byte[5];

                    listener.GetStream().Read(helloMsg, 0, 5);

                    if (!helloMsg.SequenceEqual(HelloHeader))
                        Console.WriteLine("Ip not work");

                    r.Stop();
                    Console.WriteLine("IP IS SERVER");
                })
                .Build();

            radar.Scan();
        }

        private class ObservableList<T> : List<T>
        {
            private Action<T> addCallback;

            public new void Add(T item)
            {
                base.Add(item);
                addCallback(item);
            }

            public void setAddCallback(Action<T> callback) { addCallback = callback; }
        }

        volatile ObservableList<IPAddress> clients;

        public void workAsServer()
        {
            clients = new ObservableList<IPAddress>();
            
            TcpListener server = new TcpListener(new IPEndPoint(IPAddress.Any, 3211));
            server.Start();
            server.BeginAcceptTcpClient((IAsyncResult ar) =>
            {
                TcpListener listener = (TcpListener)ar.AsyncState;
                TcpClient client = listener.EndAcceptTcpClient(ar);

                client.Client.Send(HelloHeader);
                Console.WriteLine("client accepted");
            }, server);


            clients.setAddCallback((IPAddress address) =>
            {
                server.Server.SendToAsync(new SocketAsyncEventArgs() { });
            });

            while(true)
            {
                Thread.Sleep(10000);
            }
        }
    }
}

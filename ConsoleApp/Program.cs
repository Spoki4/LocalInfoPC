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
            Info info = new Info();
            TcpListener listener = new TcpListener(IPAddress.Any, 3210);
            listener.Start();
            while (true)
            {
                Console.WriteLine("Waiting for a server found this client...");
                TcpClient server = listener.AcceptTcpClient();

                while (true)
                {
                    var computerInfo = info.getAllInfo();
                    IFormatter formatter = new BinaryFormatter();
                    Stream stream = new MemoryStream();


                    formatter.Serialize(stream, computerInfo);
                    byte[] buffer = ((MemoryStream)stream).ToArray();

                    int bytesSent = server.Client.Send(buffer);

                    if (bytesSent == 0)
                    {
                        Console.WriteLine("Disconnected from a server.");
                        server.Close();
                        break;
                    }

                    Thread.Sleep(5000);
                }
            }
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

            var radar = new LocalRadar.RadarBuilder()
                .SetRange(IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.255.255"))
                .SetPort(3210)
                .SetFrequency(15000)
                .SetFindCallback((IPAddress address) =>
                {
                    if(!clients.Contains(address))
                        clients.Add(address);
                })
                .Build();

            clients.setAddCallback((IPAddress address) =>
            {
                try { 
                    TcpClient client = new TcpClient(new IPEndPoint(address, 3210));
                    byte[] receiveBuffer = new byte[1024];
                    while(true) { 
                        NetworkStream stream = client.GetStream();
                        Int32 receiveBytes = stream.Read(receiveBuffer, 0, 1024);

                        Console.WriteLine(receiveBytes);
                        Console.WriteLine(receiveBuffer);
                    }
                } catch(Exception e)
                {
                    Console.WriteLine(e);
                }
            });

            radar.Scan();
            
        }
    }
}

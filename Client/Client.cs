using NetworkConstants;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace Client
{
    class Client
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Write `exit` or `quit` to stopping client");
            Thread networkThread = new Thread(ListeningServer);
            networkThread.Start();

            while (true)
            {
                var commandLine = Console.ReadLine().ToLower();

                if(commandLine.Contains("exit") || commandLine.Contains("quit"))
                {
                    Console.WriteLine("Stopping client...");
                    networkThread.Abort();
                    break;
                }

                Thread.Sleep(50);
            }

            networkThread.Join(1000);
        }

        private static void ListeningServer()
        {
            TcpListener listener = new TcpListener(new IPEndPoint(IPAddress.Any, 3210));
            TcpClient server = null;
            try {
                listener.Start();
                while(true)
                {
                    Console.WriteLine("Waiting server found this client...");
                    server = listener.AcceptTcpClient();
                    WorkWithServer(server);
                    Console.WriteLine("Server disconnected");
                }
            }
            catch (ThreadAbortException)
            {
                listener.Stop();
                Console.WriteLine("Stopping send data to server");
            }
        }

        struct ServerInstance
        {
            public TcpClient server;
            public byte[] command;
        }

        private static void WorkWithServer(TcpClient server)
        {
            Console.WriteLine("Server found this client");
            server.ReceiveTimeout = 1000;
            server.SendTimeout = 1000;

            ServerInstance instance = new ServerInstance();
            instance.server = server;
            

            var stream = server.GetStream();
            try { 
                while(true)
                {
                    if (!server.Connected)
                        break;

                    instance.command = new byte[1024];
                    stream.BeginRead(instance.command, 0, instance.command.Length, OnRead, instance);

                    Thread.Sleep(100);
                }
            } catch (Exception e)
            {
                Console.WriteLine("Lost connection to the server");
            }
        }

        private static void OnRead(IAsyncResult ar)
        {
            try { 
                var instance = (ServerInstance)ar.AsyncState;

                if (!instance.server.Connected)
                    return;

                var stream = instance.server.GetStream();

                stream.EndRead(ar);

                string command = Encoding.ASCII.GetString(instance.command);
                command = command.TrimEnd('\0');
                Console.WriteLine(command);

                if (command.Equals(Constants.NETWORK_PING))
                {
                    var pingBytes = Encoding.ASCII.GetBytes(Constants.NETWORK_PING);
                    stream.Write(pingBytes, 0, pingBytes.Length);
                }
                else if (command.Equals(Constants.NETWORK_STATS))
                {
                    var info = new PCInfo.Info().getAllInfo();
                    var bytes = ObjectToByteArray(info);
                    stream.Write(bytes, 0, bytes.Length);
                }
            }
            catch (Exception e)
            {
                
            }
        }

        private static byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}

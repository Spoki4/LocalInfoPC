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

        private static void WorkWithServer(TcpClient server)
        {
            Console.WriteLine("Server found this client");
            while(true)
            {
                if (!server.Connected)
                    break;

                var stream = server.GetStream();

                if (!stream.DataAvailable)
                    continue;

                var buffer = new byte[1024];
                stream.Read(buffer, 0, buffer.Length);

                string command = Encoding.ASCII.GetString(buffer);
                command = command.TrimEnd('\0');
                Console.WriteLine(command);

                if(command.Equals(Constants.NETWORK_PING))
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

                Thread.Sleep(100);
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

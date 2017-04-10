using LocalRadar;
using NetworkConstants;
using PCInfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Timers;

namespace Server.services
{
    class Computers
    {
        struct Client
        {
            public TcpClient socket;
            public byte[] receiveBytes;
            public ComputerInfo lastInfo;
        }

        private volatile List<Client> computers = new List<Client>();
        private Timer refreshTimer;

        public Computers()
        {
            var radar = new RadarBuilder()
                .SetPort(3210)
                .SetFrequency(30000)
                .SetFindCallback((TcpClient socket) =>
                {
                    foreach (var comp in computers)
                        if (socket.Equals(comp))
                            return;

                    Console.WriteLine("New computer add on address {0}", socket.Client.RemoteEndPoint);

                    Client client = new Client();
                    client.socket = socket;

                    computers.Insert(0, client);
                    RefreshData(null, null);
                })
                .SetRange(IPAddress.Parse("192.168.0.0"), IPAddress.Parse("192.168.255.255"))
                .Build();

            radar.Scan();

            refreshTimer = new Timer(5000);
            refreshTimer.Elapsed += RefreshData;
            refreshTimer.AutoReset = true;
            refreshTimer.Start();
        }

        public List<ComputerInfo> getInfo()
        {
            List<ComputerInfo> info = new List<ComputerInfo>(computers.Count);
            foreach (var comp in computers)
                info.Add(comp.lastInfo);

            return info;
        }

        private void RefreshData(object sender, ElapsedEventArgs e)
        {
            byte[] getStatsCommand = Encoding.ASCII.GetBytes(Constants.NETWORK_STATS);
            foreach (var item in computers)
            {
                if (!item.socket.Connected)
                    computers.Remove(item);

                item.socket.Client.BeginSend(getStatsCommand, 0, getStatsCommand.Length, 0, onSend, item);
            }
        }

        private void onSend(IAsyncResult ar)
        {
            Client client = (Client)ar.AsyncState;

            client.socket.Client.EndSend(ar);

            client.receiveBytes = new byte[1024];
            client.socket.Client.BeginReceive(client.receiveBytes, 0, client.receiveBytes.Length, 0, OnReceive, client);
        }

        private void OnReceive(IAsyncResult ar)
        {
            Client client = (Client)ar.AsyncState;

            int bytesReceived = client.socket.Client.EndReceive(ar);
            
            if(bytesReceived > 0)
            {
                ComputerInfo info = ByteArrayToObject<ComputerInfo>(client.receiveBytes);
                if(info != null)
                    client.lastInfo = info;
            }
        }

        private T ByteArrayToObject<T>(byte[] arrBytes) where T : class
        {
            try { 
                using (var memStream = new MemoryStream())
                {
                    var binForm = new BinaryFormatter();
                    memStream.Write(arrBytes, 0, arrBytes.Length);
                    memStream.Seek(0, SeekOrigin.Begin);
                    T obj = binForm.Deserialize(memStream) as T;
                    return obj;
                }
            }
            catch (SerializationException)
            {
                return null;
            }
        }        
    }
}

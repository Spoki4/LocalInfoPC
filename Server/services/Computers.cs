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
            public int id;
            public TcpClient socket;
            public byte[] receiveBytes;
            public ComputerInfo lastInfo;
        }

        private static volatile int counter = 0;

        private List<Client> computers = new List<Client>();
        private Timer refreshTimer;

        public Computers()
        {
            var radar = new RadarBuilder()
                .SetPort(3210)
                .SetFrequency(30000)
                .SetFindCallback((TcpClient socket) =>
                {
                    foreach (var comp in computers)
                        if (socket.Equals(comp)) {
                            Console.WriteLine("Early founded {0}", socket.Client.RemoteEndPoint);
                            return;
                        }

                    Console.WriteLine("New computer add on address {0}", socket.Client.RemoteEndPoint);

                    Client client = new Client();
                    client.id = counter++;
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
            foreach (var comp in computers) { 
                info.Add(comp.lastInfo);
            }

            return info;
        }

        private void RefreshData(object sender, ElapsedEventArgs e)
        {
            byte[] getStatsCommand = Encoding.ASCII.GetBytes(Constants.NETWORK_STATS);
            while(true) { 
                try { 
                    foreach (var item in computers)
                    {
                        if (!item.socket.Connected)
                            computers.Remove(item);
                        try
                        {
                            item.socket.Client.BeginSend(getStatsCommand, 0, getStatsCommand.Length, 0, OnSend, item);
                        }
                        catch (Exception)
                        {
                            RemoveClient(item);
                        }
                    }
                    break;
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        private void OnSend(IAsyncResult ar)
        {
            Client client = (Client)ar.AsyncState;
            try { 
                

                client.socket.Client.EndSend(ar);

                client.receiveBytes = new byte[1024];
                client.socket.Client.BeginReceive(client.receiveBytes, 0, client.receiveBytes.Length, 0, OnReceive, client);
            } catch(Exception)
            {
                RemoveClient(client);
            }
        }

        private void OnReceive(IAsyncResult ar)
        {
            Client client = (Client)ar.AsyncState;
            try {                 

                int bytesReceived = client.socket.Client.EndReceive(ar);
            
                if(bytesReceived > 0)
                {
                    ComputerInfo info = ByteArrayToObject<ComputerInfo>(client.receiveBytes);
                    if(info != null) {
                        client.lastInfo = info;
                        UpdateClient(client);
                    }

                }
            } catch(Exception)
            {
                RemoveClient(client);
            }
        }

        private void RemoveClient(Client client)
        {
            computers.Remove(client);
        }

        private void UpdateClient(Client client)
        {
            for (int i = 0; i < computers.Count; i++)
            {
                if (computers[i].id == client.id) { 
                    computers[i] = client;
                    break;
                }
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

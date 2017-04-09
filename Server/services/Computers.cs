using LocalRadar;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Timers;

namespace Server.services
{
    class Computers
    {
        private volatile List<TcpClient> computers = new List<TcpClient>();
        private Timer refreshTimer;

        public Computers()
        {
            var radar = new RadarBuilder()
                .SetPort(3210)
                .SetFrequency(30000)
                .SetFindCallback((TcpClient client) =>
                {
                    foreach (var comp in computers)
                        if (client.Equals(comp))
                            return;

                    computers.Insert(1, client);
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

        private void RefreshData(object sender, ElapsedEventArgs e)
        {
            foreach (var item in computers)
            {
                if (!item.Connected)
                    computers.Remove(item);
            }
        }
    }
}

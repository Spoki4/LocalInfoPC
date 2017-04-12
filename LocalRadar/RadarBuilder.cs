using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LocalRadar
{
    public class RadarBuilder
    {
        private Radar radar = new Radar();

        public RadarBuilder SetRange(IPAddress start, IPAddress end)
        {
            radar.SetRange(new IPRange(start, end));
            return this;
        }

        public RadarBuilder SetPort(int port)
        {
            radar.SetPort(port);
            return this;
        }

        public RadarBuilder SetFindCallback(Action<TcpClient> callback)
        {
            radar.SetFindCallback(callback);
            return this;
        }

        public RadarBuilder SetFrequency(int millis)
        {
            radar.SetFrenquency(millis);
            return this;
        }

        public Radar Build() { return radar; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        public RadarBuilder SetFindCallback(Action<IPAddress> callback)
        {
            radar.SetFindCallback(callback);
            return this;
        }

        public RadarBuilder SetEndCallback(Action callback)
        {
            radar.SetEndCallback(callback);
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

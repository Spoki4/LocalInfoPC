using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LocalRadar
{
    public class IPRange
    {
        IPAddress start;
        IPAddress end;

        public IPRange(IPAddress start, IPAddress end)
        {
            this.start = start;
            this.end = end;
        }

        public IEnumerable<IPAddress> GetIPRange()
        {
            uint sIP = ipToUint(start.GetAddressBytes());
            uint eIP = ipToUint(end.GetAddressBytes());
            while (sIP <= eIP)
            {
                yield return new IPAddress(reverseBytesArray(sIP));
                sIP++;
            }
        }
        protected uint reverseBytesArray(uint ip)
        {
            byte[] bytes = BitConverter.GetBytes(ip);
            Array.Reverse(bytes);
            return (uint)BitConverter.ToInt32(bytes, 0);
        }

        protected uint ipToUint(byte[] ipBytes)
        {
            ByteConverter bConvert = new ByteConverter();
            uint ipUint = 0;

            int shift = 24;
            foreach (byte b in ipBytes)
            {
                if (ipUint == 0)
                {
                    ipUint = (uint)bConvert.ConvertTo(b, typeof(uint)) << shift;
                    shift -= 8;
                    continue;
                }

                if (shift >= 8)
                    ipUint += (uint)bConvert.ConvertTo(b, typeof(uint)) << shift;
                else
                    ipUint += (uint)bConvert.ConvertTo(b, typeof(uint));

                shift -= 8;
            }

            return ipUint;
        }
    }
}

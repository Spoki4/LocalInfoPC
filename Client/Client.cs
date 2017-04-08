using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient listener = new TcpClient(new IPEndPoint(IPAddress.Any, 3210));
            Console.WriteLine("Waiting for a server found this client...");

           
        }
    }
}

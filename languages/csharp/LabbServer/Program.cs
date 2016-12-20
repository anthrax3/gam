using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GAM;

namespace LabbServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var a = Actor.Spawn(Actor.FromProducer(() => new TcpServerActor(new IPEndPoint(IPAddress.Any, 1337))));
            Console.ReadLine();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using GAM;

namespace LabbServer
{
    public class ClientActor : IActor
    {
        private readonly TcpClient _client;
        private PID _writeActor;

        public ClientActor(TcpClient client)
        {
            _client = client;
        }

        public Task ReceiveAsync(IContext context)
        {
            if (context.Message is Started)
            {
                Console.WriteLine("Client connected from " + _client.Client.RemoteEndPoint);
                var stream = _client.GetStream();
                var reader = new StreamReader(stream);
                var writer = new StreamWriter(stream) {AutoFlush = true};
                context.Spawn(Actor.FromProducer(() => new ReadActor(reader)));
                _writeActor = context.Spawn(Actor.FromProducer(() => new WriteActor(writer)));
            }
            else if (context.Message is string)
            {
                Console.WriteLine("Received: " + (string)context.Message);
                _writeActor.Tell("Ok");
            }
            else if (context.Message is MsgReaderClosed)
            {
                Console.WriteLine("Client disconnected");
                context.Sender.Stop();
            }
            return Actor.Done;
        }
    }
}

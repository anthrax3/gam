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
    public class TcpClientActor : IActor
    {
        private readonly TcpClient _client;
        private PID _writeActor;

        public TcpClientActor(TcpClient client)
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
                context.Spawn(Actor.FromProducer(() => new StreamReaderActor(reader)));
                _writeActor = context.Spawn(Actor.FromProducer(() => new WriteActor(writer)));
            }
            else if (context.Message is string)
            {
                Console.WriteLine("Received: " + (string)context.Message);
                _writeActor.Tell("Ok");
            }
            else if (context.Message is MsgReaderClosed)
            {
                context.Parent.Tell(new MsgClientDisconnected(context.Self, _client.Client.RemoteEndPoint));
            }
            return Actor.Done;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using GAM;

namespace LabbServer
{
    class MsgConnectionClosed
    {
        public TcpClient Client { get; }

        public MsgConnectionClosed(TcpClient client)
        {
            Client = client;
        }
    }

    public class AcceptActor : IActor
    {
        private readonly TcpListener _listener;

        public AcceptActor(TcpListener listener)
        {
            _listener = listener;
        }

        public async Task ReceiveAsync(IContext context)
        {
            if (context.Message is Started)
            {
                context.Self.Tell(new MsgContinueListen());
                await Actor.Done;
            }
            else if(context.Message is MsgContinueListen)
            {
                Console.Write("Begin accepting client...");
                var client = await _listener.AcceptTcpClientAsync();
                Console.WriteLine("connected");
                context.Parent.Tell(client);
                context.Self.Tell(new MsgContinueListen());
            }
        }

        private class MsgContinueListen
        { }
    }

    public class ServerActor : IActor
    {
        private readonly TcpListener _listener;

        public ServerActor(IPEndPoint endPoint)
        {
            _listener = new TcpListener(endPoint);
            _listener.Start();
        }

        public Task ReceiveAsync(IContext context)
        {
            if (context.Message is Started)
            {
                context.Spawn(Actor.FromProducer(() => new AcceptActor(_listener)));
            }
            else if (context.Message is TcpClient)
            {
                var client = (TcpClient) context.Message;
                context.Spawn(Actor.FromProducer(() => new ClientActor(client)));
            }
            else if (context.Message is MsgConnectionClosed)
            {
                Console.WriteLine("Connection " + ((MsgConnectionClosed)context.Message).Client.Client.RemoteEndPoint + " disconnected");
                context.Sender.Stop();
            }
            return Actor.Done;
        }
    }
}

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
    class MsgClientDisconnected
    {
        public MsgClientDisconnected(PID clientActorPid, EndPoint remoteEndPoint)
        {
            ClientActorPid = clientActorPid;
            RemoteEndPoint = remoteEndPoint;
        }

        public PID ClientActorPid { get; }
        public EndPoint RemoteEndPoint { get; }
    }

    public class TcpServerActor : IActor
    {
        private readonly TcpListener _listener;

        public TcpServerActor(IPEndPoint endPoint)
        {
            _listener = new TcpListener(endPoint);
            _listener.Start();
        }

        public Task ReceiveAsync(IContext context)
        {
            if (context.Message is Started)
            {
                context.Spawn(Actor.FromProducer(() => new TcpAcceptActor(_listener)));
            }
            else if (context.Message is TcpClient)
            {
                var client = (TcpClient) context.Message;
                var clientActorRef = context.Spawn(Actor.FromProducer(() => new TcpClientActor(client)));
                context.Parent?.Tell(new MsgClientConnected(clientActorRef, client.Client.RemoteEndPoint));
            }
            else if (context.Message is MsgClientDisconnected)
            {
                var msg = (MsgClientDisconnected) context.Message;
                msg.ClientActorPid.Stop();
                Console.WriteLine("Connection " + msg.RemoteEndPoint + " disconnected");
                
                context.Parent?.Tell(context.Message);
            }
            return Actor.Done;
        }
        private class TcpAcceptActor : IActor
        {
            private readonly TcpListener _listener;

            public TcpAcceptActor(TcpListener listener)
            {
                _listener = listener;
            }

            public async Task ReceiveAsync(IContext context)
            {
                if (context.Message is Started)
                {
                    context.Self.Tell(MsgContinueListen.Instance);
                    await Actor.Done;
                }
                else if (context.Message is MsgContinueListen)
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    context.Parent.Tell(client);
                    context.Self.Tell(MsgContinueListen.Instance);
                }
            }

            private class MsgContinueListen
            {
                public static MsgContinueListen Instance { get; } = new MsgContinueListen();
            }
        }
    }

    public class MsgClientConnected
    {
        public PID ClientActorRef { get; }
        public EndPoint RemoteEndPoint { get; }

        public MsgClientConnected(PID clientActorRef, EndPoint remoteEndPoint)
        {
            ClientActorRef = clientActorRef;
            RemoteEndPoint = remoteEndPoint;
        }
    }
}

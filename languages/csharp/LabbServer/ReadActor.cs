using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GAM;

namespace LabbServer
{
    class MsgContinueRead
    { }
    internal class MsgReaderClosed
    {
    }

    class ReadActor : IActor
    {
        private readonly StreamReader _reader;

        public ReadActor(StreamReader reader)
        {
            _reader = reader;
        }

        public Task ReceiveAsync(IContext context)
        {
            if (context.Message is Started)
            {
                context.Self.Tell(new MsgContinueRead());
            }
            else if (context.Message is MsgContinueRead)
            {
                var line = _reader.ReadLine();
                if (line == null)
                {
                    context.Parent.Tell(new MsgReaderClosed());
                }
                else
                {
                    context.Parent.Tell(line);
                    context.Self.Tell(new MsgContinueRead());
                }
            }
            return Actor.Done;
        }
    }

}

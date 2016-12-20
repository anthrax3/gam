using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GAM;

namespace LabbServer
{
    class MsgContinueRead
    {
        public static MsgContinueRead Instance { get; } = new MsgContinueRead();
    }
    internal class MsgReaderClosed
    {
        public static MsgReaderClosed Instance { get; } = new MsgReaderClosed();
    }

    class StreamReaderActor : IActor 
    {
        private readonly StreamReader _reader;

        public StreamReaderActor(StreamReader reader)
        {
            _reader = reader;
        }

        public async Task ReceiveAsync(IContext context)
        {
            if (context.Message is Started)
            {
                context.Self.Tell(new MsgContinueRead());
            }
            else if (context.Message is MsgContinueRead)
            {
                var line = await _reader.ReadLineAsync();
                if (line == null)
                {
                    context.Parent.Tell(MsgReaderClosed.Instance);
                }
                else
                {
                    context.Parent.Tell(line);
                    context.Self.Tell(MsgContinueRead.Instance);
                }
            }
            await Actor.Done;
        }
    }

}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GAM;

namespace LabbServer
{
    public class WriteActor : IActor
    {
        private readonly StreamWriter _writer;

        public WriteActor(StreamWriter writer)
        {
            _writer = writer;
        }

        public async Task ReceiveAsync(IContext context)
        {
            if (context.Message is string)
            {
                await _writer.WriteLineAsync((string) context.Message);
                //_writer.Flush();
            }
            await Actor.Done;
        }
    }
}

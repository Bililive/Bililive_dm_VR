using Bililive_dm_VR.Desktop.Model;
using BinarySerialization;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace Bililive_dm_VR.Desktop
{
    internal class RpcServer
    {
        private NamedPipeServerStream namedPipeServerStream;
        private BinarySerializer bs;

        public string PipeName { get; private set; }

        internal RpcServer(BinarySerializer binarySerializer)
        {
            bs = binarySerializer;
            PipeName = "bililivevrdm";
            if (Debugger.IsAttached)
                PipeName += "debug";
            else
                PipeName += new Random().Next().ToString();
            namedPipeServerStream = new NamedPipeServerStream(PipeName);
            Task.Run(() => namedPipeServerStream.WaitForConnection());
        }

        internal bool Send(Command command)
        {
            if (!namedPipeServerStream.IsConnected)
                return false;

            var stream = new MemoryStream();

            bs.Serialize(stream, new RpcCommand() { Command = command });

            stream.WriteTo(namedPipeServerStream);

            namedPipeServerStream.Flush();

            return true;
        }

    }
}

using Bililive_dm_VR.Desktop.Model;
using BinarySerialization;
using System;
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
            PipeName = "bililivevrdm" + new Random().Next().ToString();
            namedPipeServerStream = new NamedPipeServerStream(PipeName);
            Task.Run(() => namedPipeServerStream.WaitForConnection());
        }

        internal bool Send(Command command)
        {
            if (!namedPipeServerStream.IsConnected)
                return false;

            bs.Serialize(namedPipeServerStream, new RpcCommand() { Command = command });
            return true;
        }

    }
}

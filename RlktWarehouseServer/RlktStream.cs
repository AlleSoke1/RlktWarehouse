using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RlktWarehouseServer
{
    internal class RlktReader : BinaryReader
    {
        public RlktReader(byte[] data) : base(new MemoryStream(data)) { }
        public RlktReader(MemoryStream stream) : base(stream) { }
        public RlktReader(NetworkStream stream) : base(stream) { }
    }

    internal class RlktWriter : BinaryWriter
    {
        public RlktWriter() : base(new MemoryStream()) { }
        public RlktWriter(byte[] data) : base(new MemoryStream(data)) { }
        public RlktWriter(MemoryStream stream) : base(stream) { }

        public Stream GetStream() { return OutStream; }

        public byte[] ToArray() 
        {
            MemoryStream ms = new MemoryStream();
            OutStream.Position = 0;
            OutStream.CopyTo(ms);
            return ms.ToArray();
        }
    }
}

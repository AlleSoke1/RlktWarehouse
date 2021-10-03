using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RlktWarehouseServer
{
    internal class RlktReader : BinaryReader
    {
        public RlktReader(byte[] data) : base(new MemoryStream(data)) { }
    }

    internal class RlktWriter : BinaryWriter
    {
        public RlktWriter() : base(new MemoryStream()) { }
        public RlktWriter(byte[] data) : base(new MemoryStream(data)) { }
        public RlktWriter(MemoryStream stream) : base(stream) { }

    }
}

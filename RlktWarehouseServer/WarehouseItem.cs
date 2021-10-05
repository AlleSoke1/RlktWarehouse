using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RlktWarehouseServer
{
    internal class WarehouseItem
    {
        public string warehouseName;
        public string filename;
        public int size;
        public int version;
        public bool isCompressed;
        public uint checksum;
        public int uncompressSize;
    }
}

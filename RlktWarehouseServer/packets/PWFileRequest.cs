using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RlktWarehouseServer.packets
{
    internal class PWFileRequest
    {
        WarehouseNetClient client;
        public PWFileRequest(RlktReader reader, WarehouseNetClient netClient)
        {
            client = netClient;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RlktWarehouseServer
{
    internal class Utils
    {
        public static int MAX_WPACKET_SIZE = 65536;

        public static string GetRandomName()
        {
            return Guid.NewGuid().ToString();
        }
    }
}

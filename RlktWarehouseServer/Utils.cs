using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RlktWarehouseServer
{
    internal class Utils
    {
        public static int PACKET_HEADER_SIZE        = 8;
        public static int MAX_WPACKET_SIZE          = 65536;
        public static int MAX_WPACKET_BIG_SIZE      = 50*1024*1024;
        public static int MAX_WPACKET_SIZE_ENCAP    = MAX_WPACKET_SIZE - PACKET_HEADER_SIZE;

        public static string GetRandomName()
        {
            return Guid.NewGuid().ToString();
        }

        public static uint GetChecksum(byte[] data)
        {
            UInt64 checksum = 0;
            for (int i = 0; i < data.Length; i++)
            {
                checksum += data[i];
            }
            return (uint)(checksum % UInt32.MaxValue);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RlktWarehouseServer.packets
{
    internal class PWVersionCheck : WPacket
    {
        private WEPacketType PACKET_TYPE = WEPacketType.CHECK_FOR_UPDATES;

        int nAppId;
        int nCurAppVersion;
        WarehouseNetClient client;

        public PWVersionCheck(RlktReader reader, WarehouseNetClient netClient)
        {
            nAppId = reader.ReadInt32();
            nCurAppVersion = reader.ReadInt32();
            client = netClient;
        }

        public override bool ProcessRequest()
        {  
            //Get response
            int version = WarehouseManager.Instance.GetLatestVersionFromWarehouse(nAppId);

            //Log version
            Console.WriteLine("Client requested version check from warehouse his version[{0:D}], warehouse version[{0:D}], appId[{0:D}]", nCurAppVersion, version, nAppId);

            //Write packet
            MemoryStream stream = new MemoryStream();
            using (RlktWriter writer = new RlktWriter(stream))
            {
                writer.Write(version);
            }

            //Send packet
            client.OnSendPacket(PACKET_TYPE, stream.ToArray());

            return true;
        }

    }
}

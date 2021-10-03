using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RlktWarehouseServer.packets
{
    internal class PWFileDeposit : WPacket
    {
        int nAppId;
        int nNewAppVersion;
        int appSize;
        byte[] appData;
        WarehouseNetClient client;

        public PWFileDeposit(RlktReader reader, WarehouseNetClient netClient)
        {
            nAppId         = reader.ReadInt32();
            nNewAppVersion = reader.ReadInt32();
            appSize        = reader.ReadInt32();
            appData        = reader.ReadBytes(appSize);

            client = netClient;
        }

        public override bool ProcessRequest()
        {
            Warehouse warehouse = WarehouseManager.Instance.GetWarehouseByAppId((WarehouseAppId)nAppId);
            if (warehouse == null)
            {
                Console.WriteLine("Failed to get warehouse, reqAppId[{0:D}], newAppVersion[{0:D}]", nAppId, nNewAppVersion);
            }

            warehouse.AddItemToWarehouse(appData, appSize, "test", nNewAppVersion);

            using (RlktWriter writer = new RlktWriter())
            {
                writer.Write(true);
                client.OnSendPacket(WEPacketType.FILE_DEPOSIT, writer.ToArray());
            }

            return true;
        }

    }
}

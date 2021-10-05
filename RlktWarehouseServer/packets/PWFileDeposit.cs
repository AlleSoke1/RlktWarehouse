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
        string executableName;
        bool isCompressed;
        int appSize;
        int appOrigSize;
        byte[] appData;
        WarehouseNetClient client;

        public PWFileDeposit(RlktReader reader, WarehouseNetClient netClient)
        {
            nAppId         = reader.ReadInt32();
            nNewAppVersion = reader.ReadInt32();
            executableName = reader.ReadString();
            isCompressed   = reader.ReadBoolean();
            appOrigSize    = reader.ReadInt32();
            appSize        = reader.ReadInt32();
            appData        = reader.ReadBytes(appSize);

            client = netClient;
        }

        public override bool ProcessRequest()
        {
            bool result = false;
            Warehouse warehouse = WarehouseManager.Instance.GetWarehouseByAppId((WarehouseAppId)nAppId);
            if (warehouse != null)
            {
                result = warehouse.AddItemToWarehouse(appData, appSize, executableName, nNewAppVersion, isCompressed, appOrigSize);
            }

            using (RlktWriter writer = new RlktWriter())
            {
                writer.Write(result);
                client.OnSendPacket(WEPacketType.FILE_DEPOSIT, writer.ToArray());
            }

            return false;
        }

    }
}

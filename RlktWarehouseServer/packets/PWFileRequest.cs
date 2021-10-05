using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RlktWarehouseServer.packets
{
    internal class PWFileRequest : WPacket
    {
        int nAppId;
        int nReqAppVersion;
        WarehouseNetClient client;
        public PWFileRequest(RlktReader reader, WarehouseNetClient netClient)
        {
            nAppId          = reader.ReadInt32();
            nReqAppVersion  = reader.ReadInt32();

            client = netClient;
        }

        public override bool ProcessRequest()
        {
            //Check authentification of user
            if (!client.handshakeCompleted)
            {
                Console.WriteLine("Client requests file without a valid handshake.");
                return false;
            }

            //Get warehouse
            Warehouse warehouse = WarehouseManager.Instance.GetWarehouseByAppId((WarehouseAppId)nAppId);
            if (warehouse == null)
            {
                Console.WriteLine("Failed to get warehouse, reqAppId[{0:D}], reqAppVersion[{0:D}]", nAppId, nReqAppVersion);
                return false;
            }

            //Get warehouse item info
            WarehouseItem warehouseItem = warehouse.GetLatestVersionItem();
            if (warehouseItem == null)
            {
                Console.WriteLine("Failed to get item from warehouse, reqAppId[{0:D}], reqAppVersion[{0:D}]", nAppId, nReqAppVersion);
                return false;
            }

            //Step 1 - send file info
            using (RlktWriter writer = new RlktWriter())
            {
                writer.Write((uint)warehouseItem.size);
                writer.Write(warehouseItem.isCompressed);
                writer.Write(warehouseItem.uncompressSize);

                client.OnSendPacket(WEPacketType.XFER_START, writer.ToArray());
            }

            //Step 2 - send file chunks
            string warehouseFile = Path.Combine(warehouse.GetWarehousePath(), warehouseItem.warehouseName);
            using (var file = File.OpenRead(warehouseFile))
            {
                int bytesRead;
                var buffer = new byte[Utils.MAX_WPACKET_SIZE_ENCAP];
                while ((bytesRead = file.Read(buffer, 0, buffer.Length)) > 0)
                {
                    using (RlktWriter writer = new RlktWriter())
                    {
                        writer.Write(buffer, 0, bytesRead);
                        client.OnSendPacket(WEPacketType.XFER_DATACHUNK, writer.ToArray());
                    }
                }
            }


            //Step 3 - send finish packet
            using (RlktWriter writer = new RlktWriter())
            {
                writer.Write(warehouseItem.checksum);
                client.OnSendPacket(WEPacketType.XFER_END, writer.ToArray());
            }

            return true;
        }
    }
}

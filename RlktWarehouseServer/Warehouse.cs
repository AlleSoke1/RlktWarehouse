using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RlktWarehouseServer
{
    public enum WarehouseAppId
    {
        AppId1,
        AppId2,
        AppId3,
        AppId4,
        AppId5,
        AppIdMAX
    }

    internal class Warehouse
    {
        private string warehouseFile = "warehouse.rlkt";
        private string warehouseStorage = ".\\data";
        public WarehouseAppId warehouseAppId;

        List<WarehouseItem> items = null;

        private int reservedSize = 95; //-bool-uint //Original value was 100.

        public Warehouse(WarehouseAppId warehouseId)
        {
            this.warehouseAppId = warehouseId;
            LoadItems();
        }

        public bool LoadItems()
        {
            items?.Clear();

            items = new List<WarehouseItem>();

            if (File.Exists(GetWarehouseConfig()) == false)
                return false;

            using (RlktReader reader = new RlktReader(File.ReadAllBytes(GetWarehouseConfig())))
            {
                int itemCount = reader.ReadInt32();
                for(int i=0;i<itemCount; i++)
                {
                    WarehouseItem item = new WarehouseItem();

                    item.warehouseName  = reader.ReadString();
                    item.filename       = reader.ReadString();
                    item.size           = reader.ReadInt32();
                    item.version        = reader.ReadInt32();
                    item.isCompressed   = reader.ReadBoolean();
                    item.checksum       = reader.ReadUInt32();

                    reader.BaseStream.Seek(reservedSize, SeekOrigin.Current);

                    items.Add(item);
                }
            }            
                
            return true;
        }

        public bool SaveItems()
        {
            using (RlktWriter writer = new RlktWriter())
            {
                writer.Write(items.Count);
                foreach (WarehouseItem item in items)
                {
                    writer.Write(item.warehouseName);
                    writer.Write(item.filename);
                    writer.Write(item.size);
                    writer.Write(item.version);
                    writer.Write(item.isCompressed);
                    writer.Write(item.checksum);

                    writer.Write(new string('\0', reservedSize).ToArray<char>());
                }
            }
            
            return true;
        }

        public bool AddItemToWarehouse(byte[] data, int size, string origName, int version)
        {
            //Get random entry name
            string entryName = Utils.GetRandomName();

            //Check and create directory if it doesn't exist.
            if (Directory.Exists(GetWarehousePath()) == false)
                Directory.CreateDirectory(GetWarehousePath());

            //Save file
            File.WriteAllBytes(Path.Combine(GetWarehousePath(), entryName), data);

            //Add an warehouse entry
            WarehouseItem item = new WarehouseItem();

            item.warehouseName = entryName;
            item.filename = origName;
            item.size = size;
            item.version = version;

            items.Add(item);

            //Save Warehouse config
            SaveItems();

            return true;
        }

        public int GetLatestVersion()
        {
            int version = 0;

            foreach (WarehouseItem item in items)
            {
                if (item.version > version)
                    version = item.version;
            }

            return version;
        }

        public WarehouseItem GetLatestVersionItem()
        {
            WarehouseItem _item = null;

            int version = 0;
            foreach (WarehouseItem item in items)
            {
                if (item.version > version)
                {
                    version = item.version;
                    _item = item;
                }   
            }

            return _item;
        }

        public string GetWarehousePath()
        {
            return Path.Combine(warehouseStorage, warehouseAppId.ToString());
        }

        public string GetWarehouseConfig()
        {
            return Path.Combine(GetWarehousePath(), warehouseFile);
        }
    }
}

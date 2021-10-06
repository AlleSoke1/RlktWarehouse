using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RlktWarehouseServer
{
    internal class WarehouseManager
    {
        public static WarehouseManager Instance = null;

        List<Warehouse> list = new List<Warehouse>();
        public WarehouseManager()
        {
            list.Add(new Warehouse(WarehouseAppId.AppId1));
            list.Add(new Warehouse(WarehouseAppId.AppId2));
            list.Add(new Warehouse(WarehouseAppId.AppId3));
            list.Add(new Warehouse(WarehouseAppId.AppId4));
            list.Add(new Warehouse(WarehouseAppId.AppId5));
        }

        public static void Initialize()
        {
            if (Instance == null)
                Instance = new WarehouseManager();
        }

        public int GetLatestVersionFromWarehouse(int warehouseAppId)
        {
            foreach (Warehouse warehouse in list)
            {
                if((int)warehouse.warehouseAppId == warehouseAppId)
                {
                    return warehouse.GetLatestVersion();
                }
            }

            return -1;
        }

        public Warehouse GetWarehouseByAppId(WarehouseAppId appId)
        {
            foreach (Warehouse warehouse in list)
            {
                if (warehouse.warehouseAppId == appId)
                    return warehouse;
            }

            return null;
        }
    }
}

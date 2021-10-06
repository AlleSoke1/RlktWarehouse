using System;

namespace RlktWarehouseServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            WarehouseManager.Initialize();

            WarehouseNetServer netServer = new WarehouseNetServer(9666);
            netServer.StartServer();
            netServer.MainServer();
        }
    }
}

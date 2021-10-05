using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RlktWarehouseServer
{
    internal class WarehouseNetServerInfo
    {
        public string bindIp { get; set; }
        public int bindPort { get; set; }
        public int clientsCounter { get; set; } = 0;
    }

    internal class WarehouseNetServer
    {
        WarehouseNetServerInfo serverInfo; 
        TcpListener serverSocket = null;

        public WarehouseNetServer( int bindPort, string bindIp = "0.0.0.0" )
        {
            serverInfo = new WarehouseNetServerInfo();
            serverInfo.bindIp = bindIp; 
            serverInfo.bindPort = bindPort;
        }

        public bool StartServer()
        {
            serverSocket = new TcpListener(IPAddress.Parse(serverInfo.bindIp), serverInfo.bindPort);
            serverSocket.Start();

            return true;
        }

        public bool StopServer()
        {
            if (serverSocket == null)
                return false;

            serverSocket.Stop();

            return true;
        }

        public void MainServer()
        {
            while (true)
            {
                TcpClient tcpclient = serverSocket.AcceptTcpClient();

                WarehouseNetClient client = new WarehouseNetClient();
                client.HandleClient(tcpclient);

                serverInfo.clientsCounter += 1;
            }
        }

    }
}

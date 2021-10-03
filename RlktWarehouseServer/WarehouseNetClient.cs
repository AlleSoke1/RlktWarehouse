using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RlktWarehouseServer.packets;

namespace RlktWarehouseServer
{
    internal class WarehouseNetClient
    {
        TcpClient clientSocket = default(TcpClient);
        Thread recvThread = null;
        public bool handshakeCompleted = false;

        public WarehouseNetClient()
        {
        }

        public void HandleClient(TcpClient client)
        {
            clientSocket = client;

            recvThread = new Thread(RecvThread);
            recvThread.Start();

            //send handshake
            OnSendPacket(WEPacketType.HANDSHAKE);
        }

        public bool OnSendPacket(WEPacketType packetType, byte[] data = null)
        {
            int packetLen = data != null ? data.Length : 0;
            byte[] packetData = new byte[packetLen + 8];
            using (RlktWriter writer = new RlktWriter(packetData))
            {
                writer.Write((int)packetType);
                writer.Write((ushort)(packetLen + 8));

                if(data != null)
                    writer.Write(data);
            }

            try
            {
                NetworkStream networkStream = clientSocket.GetStream();
                networkStream.Write(packetData, 0, packetData.Length);
                networkStream.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine(" >> " + ex.ToString());
            }

            return true;
        }

        private bool OnRecvPacket(byte[] data)
        {
            if (data.Length < 8)
                return false;

            using (RlktReader reader = new RlktReader(data))
            {
                int type = reader.ReadInt32();
                int size = reader.ReadInt16();

                Console.WriteLine("RecvPacket Type[{type,0:D}] Size[{size,0:D}]");

                WPacket packet = new();

                switch((WEPacketType)type)
                {
                    case WEPacketType.HANDSHAKE:         handshakeCompleted = true; break;
                    case WEPacketType.CHECK_FOR_UPDATES: packet = new PWVersionCheck(reader, this); break;
                    case WEPacketType.FILE_REQUEST:      packet = new PWFileTransfer(reader, this); break;
                }

                if (packet.ProcessRequest() == false)
                    return false;
            }

            return true;
        }

        private void RecvThread()
        {
            int requestCount = 0;
            byte[] bytesFrom = new byte[Utils.MAX_WPACKET_SIZE];

            while (true)
            {
                try
                {
                    requestCount++;
                    NetworkStream networkStream = clientSocket.GetStream();
                    networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);

                    if (OnRecvPacket(bytesFrom) == false)
                    {
                        Console.WriteLine(" >> Failed processing packet, connection closed.");
                        clientSocket.Close();
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" >> " + ex.ToString());
                    break;
                }
            }
        }
    }
}

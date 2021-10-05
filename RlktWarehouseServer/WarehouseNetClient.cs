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

            //clientSocket.NoDelay = true; 

            recvThread = new Thread(RecvThread);
            recvThread.Start();

            //send handshake
            OnSendPacket(WEPacketType.HANDSHAKE);
        }

        public bool OnSendPacket(WEPacketType packetType, byte[] data = null)
        {
            int packetLen = data != null ? data.Length : 0;
            MemoryStream stream = new MemoryStream();
            RlktWriter writer = new RlktWriter(stream);

            writer.Write((int)packetType);
            writer.Write((int)(packetLen + Utils.PACKET_HEADER_SIZE));

            if(data != null)
                writer.Write(data);

            try
            {
                NetworkStream networkStream = clientSocket.GetStream();
                networkStream.Write(stream.ToArray(), 0, (int)stream.Length);
                networkStream.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine(" >> " + ex.ToString());
            }

            return true;
        }

        private bool OnRecvPacket(WEPacketType type, byte[] data)
        {
            using (RlktReader reader = new RlktReader(data))
            {
                reader.BaseStream.Seek(8, SeekOrigin.Current);

                WPacket packet = new();

                switch(type)
                {
                    case WEPacketType.HANDSHAKE:         handshakeCompleted = true; break;
                    case WEPacketType.CHECK_FOR_UPDATES: packet = new PWVersionCheck(reader, this); break;
                    case WEPacketType.FILE_REQUEST:      packet = new PWFileRequest(reader, this); break;
                    case WEPacketType.FILE_DEPOSIT:      packet = new PWFileDeposit(reader, this); break;
                }

                if (packet.ProcessRequest() == false)
                    return false;
            }

            return true;
        }

        private void RecvThread()
        {
            int requestCount = 0;
            byte[] data = new byte[Utils.MAX_WPACKET_SIZE];
            RlktWriter writer = new RlktWriter();
            int packetType = 0;
            int packetSize = 0;

            while (true)
            {
                try
                {
                    requestCount++;

                    if (clientSocket.Connected == false)
                        break;

                    NetworkStream networkStream = clientSocket.GetStream();
                    int readSize = networkStream.Read(data, 0, (int)Utils.MAX_WPACKET_SIZE);
                    if (readSize <= 0)
                        continue;
             
                    using (RlktReader reader = new RlktReader(data))
                    {
                        if (writer.BaseStream.Length == 0)
                        {
                            packetType = reader.ReadInt32();
                            packetSize = reader.ReadInt32();
                        }

                        writer.Write(data, (int)writer.BaseStream.Position, packetSize);

                        if (packetSize > readSize)
                            continue;
                    }

                    //Process the packet
                    if (OnRecvPacket((WEPacketType)packetType, writer.ToArray()) == false)
                    {
                        Console.WriteLine(" >> Failed processing packet, connection closed.");
                        clientSocket.Close();
                        break;
                    }

                    writer = new RlktWriter();
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

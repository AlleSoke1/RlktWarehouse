using System;
using System.IO;
using System.Net.Sockets;

namespace RlktWarehousePublisher
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 5)
            {
                Console.WriteLine("Usage: RlktWarehousePublisher.exe <ip> <port> <appid> <version> <executablePath>");
                return;
            }

            string ip       = args[0];
            string port     = args[1]; 
            string appId    = args[2]; 
            string version  = args[3]; 
            string exePath  = args[4];

            if(File.Exists(exePath) == false)
            {
                Console.WriteLine("Coult not open file {0:s}.", exePath);
                return;
            }

            try
            {
                TcpClient tcpClient = new TcpClient(ip, int.Parse(port));
                tcpClient.SendTimeout = 600000;
                tcpClient.ReceiveTimeout = 600000;

                byte[] fileData = File.ReadAllBytes(exePath);
                MemoryStream stream = new MemoryStream();
                using (BinaryWriter bw = new BinaryWriter(stream))
                {
                    bw.Write(7);
                    bw.Write((ushort)6); // not needed
                    bw.Write(int.Parse(appId));
                    bw.Write(int.Parse(version));
                    bw.Write(fileData.Length);
                    bw.Write(fileData);
                    tcpClient.Client.Send(stream.ToArray());
                }


                byte[] recvData = new byte[1024];  
                NetworkStream networkStream = tcpClient.GetStream();
                int recvSize = networkStream.Read(recvData, 0, 1024);

                using (BinaryReader reader = new BinaryReader(new MemoryStream(recvData)))
                {
                    reader.ReadInt32();
                    reader.ReadUInt16();
                    bool result = reader.ReadBoolean();

                    if (result)
                    {
                        Console.WriteLine("Successful published app with version [{0:s}].", version);
                    }
                    else
                    {
                        Console.WriteLine("Failed publishing app with version [{0:s}].", version);
                    }
                }

                tcpClient.Client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}

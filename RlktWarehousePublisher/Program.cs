using System;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace RlktWarehousePublisher
{
    internal class Program
    {
        static int GetVersionFromFile(string filePath)
        {
            int version = 0;

            var text = File.ReadAllText(filePath);

            string pattern = @"[0-9]+";
            Regex rg = new Regex(pattern);
            MatchCollection matchCollection = rg.Matches(text);

            if (matchCollection.Count > 0)
            {
                int.TryParse(matchCollection[0].Value, out version);
            }

            return version;
        }

        static bool SetVersionToFile(string filePath, int nVersion)
        {
            bool bResult = false;

            File.WriteAllText(filePath, String.Format("static const int g_whAppVersion = {0:D};", nVersion));

            return bResult;
        }

        static void Main(string[] args)
        {
            if (args.Length != 6)
            {
                Console.WriteLine("Usage: RlktWarehousePublisher.exe <ip> <port> <appid> <increaseVersion?> <versionPath> <executablePath>");
                return;
            }

            string ip       = args[0];
            string port     = args[1]; 
            string appId    = args[2]; 
            bool verInc     = bool.Parse(args[3]); 
            string verPath  = args[4]; 
            string exePath  = args[5];

            if(File.Exists(exePath) == false)
            {
                Console.WriteLine("[Executable] Could not open file {0:s}.", exePath);
                return;
            }

            if (File.Exists(verPath) == false)
            {
                Console.WriteLine("[Version] Could not open file {0:s}.", verPath);
                return;
            }

            int version = GetVersionFromFile(verPath);
            if(version == 0)
            {
                Console.WriteLine("[Version] Could not get version from file {0:s}.", verPath);
                return;
            }

            try
            {
                byte[] recvData = new byte[1024];

                TcpClient tcpClient = new TcpClient(ip, int.Parse(port));
                tcpClient.SendTimeout = 600000;
                tcpClient.ReceiveTimeout = 600000;

                //Recv Handshake
                NetworkStream networkStream = tcpClient.GetStream();
                int recvSize = networkStream.Read(recvData, 0, 1024);

                //Send file
                string fileName = Path.GetFileName(exePath);
                bool isCompressed = true;
                byte[] fileData = File.ReadAllBytes(exePath);

                byte [] buffer = new byte[fileData.Length];
                ZstdNet.Compressor compressor = new ZstdNet.Compressor();
                int compressedSize = compressor.Wrap(fileData, buffer);

                MemoryStream stream = new MemoryStream();
                using (BinaryWriter bw = new BinaryWriter(stream))
                {
                    int packetSize = sizeof(int) * 6 + sizeof(bool) + (fileName.Length*2+2) + compressedSize;

                    bw.Write(7);
                    bw.Write(1234); //Placeholder, to be written at the end.
                    bw.Write(int.Parse(appId));
                    bw.Write(version);
                    bw.Write(fileName);
                    bw.Write(isCompressed);
                    bw.Write(fileData.Length);
                    bw.Write(compressedSize);
                    bw.Write(buffer, 0, compressedSize);

                    //Write the length of packet
                    bw.BaseStream.Position = 4;
                    bw.Write(bw.BaseStream.Length);

                    tcpClient.Client.Send(stream.ToArray());
                }

                //Recv Response
                networkStream = tcpClient.GetStream();
                recvSize = networkStream.Read(recvData, 0, 1024);

                using (BinaryReader reader = new BinaryReader(new MemoryStream(recvData)))
                {
                    //Skip these two.
                    reader.ReadInt32(); //Packet Type
                    reader.ReadInt32(); //Packet Size

                    bool result = reader.ReadBoolean();

                    if (result)
                    {
                        Console.WriteLine("Successful published app with version [{0:d}].", version);
                        
                        if (verInc)
                        {
                            version++;
                            SetVersionToFile(verPath, version);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed publishing app with version [{0:d}].", version);
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

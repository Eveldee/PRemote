using System;

using PRemote.Shared;
using PRemote.Shared.Extensions;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using MessagePack;
using System.Linq;

namespace PRemote.Client.CLI
{
    class Program
    {
        static IPEndPoint ServerIP;
        static TcpClient tcpClient;
        static NetworkStream networkStream;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting CLI Client...");

            IniConnection();
        }

        static void IniConnection()
        {
            Console.WriteLine("Waiting for server...");

            UdpClient udpClient = new UdpClient(PConnection.UDPPort)
            {
                EnableBroadcast = true
            };

            ServerIP = new IPEndPoint(IPAddress.Any, PConnection.UDPPort);
            byte[] header;

            while (true)
            {
                header = udpClient.Receive(ref ServerIP);

                if (header.SequenceEqual(PConnection.UDPPacketData))
                    break;
            }

            Console.WriteLine("Found server on " + ServerIP.ToString());

            tcpClient = new TcpClient(ServerIP.Address.ToString(), PConnection.TCPPort);
            Console.WriteLine("Connected to " + tcpClient.Client.RemoteEndPoint.ToString());
            networkStream = tcpClient.GetStream();

            Thread transferThread = new Thread(TransferThread);
            transferThread.Start();
        }

        static void TransferThread()
        {
            byte[] buffer = new byte[PConnection.BufferSize];

            Console.WriteLine("Receiving capabilities...");
            networkStream.Read(buffer, 0, 8);
            long lenght = BitConverter.ToInt64(buffer, 0);
            MemoryStream memoryStream = new MemoryStream();

            // Receive Header
            while (memoryStream.Length < lenght)
            {
                int received = networkStream.Read(buffer, 0, buffer.Length);
                memoryStream.Write(buffer, 0, received);
            }

            CameraCapabilities capabilities = MessagePackSerializer.Deserialize<CameraCapabilities>(memoryStream);
            memoryStream.Close();
            Console.WriteLine();
            WriteCapabilities(capabilities);
            Console.WriteLine();

            Console.WriteLine("Connected to server, you can now send request");

            string input;

            while ((input = Console.ReadLine()) != "exit")
            {
                MemoryStream data = new MemoryStream();
                string arg = input.Split(' ').SubArray(2)[0];
                PDataType pDataType;
                object value;

                switch (input)
                {
                    case "picture":
                        pDataType = PDataType.Picture;
                        value = null;
                        break;
                    case "iso":
                        pDataType = PDataType.ISO;
                        value = int.Parse(arg);
                        break;
                    case "aperture":
                        pDataType = PDataType.Aperture;
                        value = double.Parse(arg);
                        break;
                    case "shutter":
                        pDataType = PDataType.ShutterSpeed;
                        value = arg;
                        break;
                    default:
                        continue;
                }

                MessagePackSerializer.Serialize(data, new PPacket(pDataType, value));
                long dataLenght = data.Length;

                networkStream.Write(BitConverter.GetBytes(dataLenght), 0, 8);

                while (data.Position < dataLenght)
                {
                    //? Get left bytes to send
                    int leftBytes = (int)(lenght - data.Position);
                    if (leftBytes > PConnection.BufferSize)
                        leftBytes = PConnection.BufferSize;

                    data.Read(buffer, 0, leftBytes);
                    networkStream.Write(buffer, 0, leftBytes);
                }
            }
        }

        static void WriteCapabilities(CameraCapabilities capabilities)
        {
            Console.WriteLine($@"{capabilities.Name}:\n
                {capabilities.BatteryLevel}%\n
                Configure: {capabilities.CanBeConfigured}\n
                Preview: {capabilities.CanCapturePreviews}\n
                Iso: {capabilities.SupportedIsoSpeeds.Concat(" ")}\n
                Apertures: {capabilities.SupportedApertures.Concat(" ")}\n
                ShutterSpeeds: {capabilities.SupportedShutterSpeeds.Concat(" ")}");
        }
    }
}

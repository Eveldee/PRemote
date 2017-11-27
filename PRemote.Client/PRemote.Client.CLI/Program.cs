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

            // Receive Header
            int received = networkStream.Read(buffer, 0, buffer.Length);
            Console.WriteLine("Received length: " + received);

            CameraCapabilities packetCapabilities = MessagePackSerializer.Deserialize<CameraCapabilities>(buffer.SubArray(0, received));
            Console.WriteLine();
            WriteCapabilities(packetCapabilities);
            Console.WriteLine();

            Console.WriteLine("Connected to server, you can now send request");

            string input;

            while ((input = Console.ReadLine()) != "exit")
            {
                string arg = "";
                try
                {
                    arg = input.Split(' ')[1];
                }
                catch (Exception) { }
                PDataType pDataType;
                object value;

                switch (input.Split(' ')[0])
                {
                    case "picture":
                        pDataType = PDataType.Picture;
                        value = "";
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

                byte[] data = MessagePackSerializer.Serialize(new PPacket(pDataType, value));
                Console.WriteLine($"Sending {data.Length} bytes");

                networkStream.Write(data, 0, data.Length);
            }
        }

        static void WriteCapabilities(CameraCapabilities capabilities)
        {
            Console.WriteLine($@"{capabilities.Name}:\n
            {capabilities.BatteryLevel} %\n
            Configure: {capabilities.CanBeConfigured}\n
            Preview: {capabilities.CanCapturePreviews}\n
            Iso: {capabilities.SupportedIsoSpeeds.Concat(" ")}\n
            Apertures: {capabilities.SupportedApertures.Concat(" ")}\n
            ShutterSpeeds: {capabilities.SupportedShutterSpeeds.Concat(" ")}");
        }
    }
}
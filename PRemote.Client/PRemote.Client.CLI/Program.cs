using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System.Linq;

using MessagePack;
using PRemote.Shared;
using PRemote.Shared.Extensions;

namespace PRemote.Client.CLI
{
    class Program
    {
        static IPEndPoint _serverIP;
        static TcpClient _tcpClient;
        static PacketStream _packetStream;

        static void Main(string[] args)
        {
            // Start here
            Console.WriteLine("Starting CLI Client...");

            IniConnection();
        }

        static void IniConnection()
        {
            // Ini all
            Console.WriteLine("Waiting for server...");

            // Find discovered server
            UdpClient udpClient = new UdpClient(PConnection.UDPPort)
            {
                EnableBroadcast = true
            };

            _serverIP = new IPEndPoint(IPAddress.Any, PConnection.UDPPort);
            byte[] header;

            while (true)
            {
                header = udpClient.Receive(ref _serverIP);

                if (header.SequenceEqual(PConnection.UDPPacketData))
                    break;
            }

            Console.WriteLine("Found server on " + _serverIP.ToString());

            // Connect to the server
            _tcpClient = new TcpClient(_serverIP.Address.ToString(), PConnection.TCPPort);

            Console.WriteLine("Connected to " + _tcpClient.Client.RemoteEndPoint.ToString());

            _packetStream = new PacketStream(_tcpClient.GetStream());

            Thread transferThread = new Thread(TransferThread);
            transferThread.Start();
        }

        static void TransferThread()
        {
            // Receive capabilities
            Console.WriteLine("Receiving capabilities...");

            try
            {
                CameraCapabilities capabilities = _packetStream.Receive<CameraCapabilities>();

                // Write Capabilities
                Console.WriteLine();
                WriteCapabilities(capabilities);
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Invalid configuration, skipping it...\n");
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("Connected to server, you can now send request");

            string input;

            // Send commands
            while ((input = Console.ReadLine()) != "exit")
            {
                string arg = "";
                PDataType pDataType;
                object value;
                string[] split = input.Split(' ');

                // Check invalid command
                if (split.Length < 1)
                {
                    Console.WriteLine("Invalid command");
                    continue;
                }

                arg = split[1];

                switch (input.Split(' ')[0])
                {
                    case "picture":
                        pDataType = PDataType.Picture;
                        value = int.Parse(arg);
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
                        Console.WriteLine("Invalid Command");
                        continue;
                }

                Console.WriteLine("Sending packet...");
                _packetStream.Send(new PPacket(pDataType, value));
                Console.WriteLine("Packet sent");
            }
        }

        static void WriteCapabilities(CameraCapabilities capabilities)
        {
            Console.WriteLine($@"{capabilities.Name}:
            {capabilities.BatteryLevel} %
            Configure: {capabilities.CanBeConfigured}
            Preview: {capabilities.CanCapturePreviews}
            Iso: {capabilities.SupportedIsoSpeeds.Concat(" ")}
            Apertures: {capabilities.SupportedApertures.Concat(" ")}
            ShutterSpeeds: {capabilities.SupportedShutterSpeeds.Concat(" ")}");
        }
    }
}
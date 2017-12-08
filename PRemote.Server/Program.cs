using System;
using System.Collections.Generic;
using System.Devices;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

using MessagePack;
using PRemote.Shared;
using PRemote.Server.Extensions;
using PRemote.Shared.Extensions;

namespace PRemote.Server
{
    class Program
    {
        // Fields
        static bool IsStarted { get; set; } = true;
        static IPEndPoint BroadcastIP { get; } = new IPEndPoint(IPAddress.Broadcast, PConnection.UDPPort);
        static List<Camera> CameraList { get; set; } = new List<Camera>();

        static void Main(string[] args) // Main Method
        {
            Console.WriteLine("[PRemote] Starting PRemote server...");

            // Getting Cameras
            Thread cameraThread = new Thread(CameraThread);
            cameraThread.Start();
            // Wait 'till one camera is connected
            while (CameraList.Count < 1)
                Thread.Sleep(300);

            // Used for auto-connection
            Thread broadcastThread = new Thread(BroadcastThread);
            broadcastThread.Start();

            // Used to Receive data from Client
            Thread connectionThread = new Thread(ConnectionThread);
            connectionThread.Start();

            // Wait exit
            while (Console.ReadLine() != "exit")
                ;

            IsStarted = false;
            Environment.Exit(0);
        }

        static async void CameraThread() // Search for camera
        {
            Console.WriteLine("[Camera Thread] Listening for remote DSLR");

            while (IsStarted)
            {
                // Error: fix this plz
                // Todo: This... It don't walk, neither run =)
                // Delete disconnected Camera & Camera that can't be remote controlled
                //for (int i = 0; i < CameraList.Count; i++)
                //{
                //    try
                //    {
                //        if (!CameraList[i].CanCaptureImages)
                //            throw new Exception();

                //        await CameraList[i].GetLensNameAsync();
                //    }
                //    catch (Exception)
                //    {
                //        Console.WriteLine($"[Camera Thread] Removed {CameraList[i].Name}");
                //        CameraList.RemoveAt(i);
                //    }
                //}


                foreach (Camera camera in await Camera.GetCamerasAsync())
                {
                    // If Camera isn't in the list, add it
                    if (!CameraList.Contains(camera, new CameraCompare()))
                    {
                        Console.WriteLine($"[Camera Thread] Added {camera.Name}");
                        CameraList.Add(camera);
                    }
                }

                await Task.Delay(5000);
            }

        }

        static async void BroadcastThread() // Send broadcast packets through UDP
        {
            UdpClient udpClient = new UdpClient(0);
            Console.WriteLine("[UDP Thread] Sending broadcast packets...");

            while (IsStarted)
            {
                await udpClient.SendAsync(PConnection.UDPPacketData, PConnection.UDPPacketDataLenght, BroadcastIP);
                await Task.Delay(2000);
            }
        }

        static async void ConnectionThread() // Wait TCP connection from a client
        {
            // Start tcpListener
            IPEndPoint iP = new IPEndPoint(IPAddress.Any, PConnection.TCPPort);
            TcpListener tcpListener = new TcpListener(iP);
            tcpListener.Start();
            Console.WriteLine("[TCP Thread]Listening on " + iP.ToString());

            // Client loop
            while (IsStarted)
            {
                // Accept connection
                Console.WriteLine("[TCP Thread] Waiting client...");

                TcpClient client = tcpListener.AcceptTcpClient();
                var networkStream = client.GetStream();

                Console.WriteLine("[TCP Thread] Accepted conneciton from " + client.Client.RemoteEndPoint.ToString());

                // Send capabilities to Client
                Console.WriteLine("[TCP Thread] Sending capabilities...");

                PacketStream packetStream = new PacketStream(networkStream);
                await packetStream.SendAsync((await CameraList[0].GetCameraCapabilitiesAsync()));

                Console.WriteLine("[TCP Thread] Capabilities sent");

                // Start Transfer Thread
                Thread dataThread = new Thread(TransferThread);
                dataThread.Start(packetStream);
            }
        }

        static async void TransferThread(object pPacketStream) // Receive data from client
        {
            Console.WriteLine("[Client Thread] Receiving data from client.");
            PacketStream packetStream = (PacketStream)pPacketStream;

            while (IsStarted)
            {
                try
                {
                    // Read data
                    Console.WriteLine("[Client Thread] Waiting packet");
                    var packet = await packetStream.ReceiveAsync<PPacket>();
                    Console.WriteLine("[Client Thread] Received packet: " + packet.ToString());

                    // Set setting
                    await SetSetting(packet);
                    Console.WriteLine("[Client Thread] Setting set");
                }
                catch (Exception e)
                {
                    Console.WriteLine("[Client Thread] Received invalid data or client disconnected: " + e.Message);
                    break;
                }
            }
            Console.WriteLine("Disconnected a client.");
        }

        static async Task SetSetting(PPacket packet) // Set a setting to all connected Camera
        {
            // Read a setting from a packet and try to set it
            Console.WriteLine("[Command Thread] Executing command...");
            for (int i = 0; i < CameraList.Count; i++)
            {
                Console.WriteLine($"[Command Thread]({i}) Camera: {CameraList[i].Name}");
                try
                {
                    switch (packet.SettingType)
                    {
                        case PDataType.Picture:
                            await CameraList[i].CaptureImageAsync();
                            break;
                        case PDataType.Aperture:
                            await CameraList[i].SetApertureAsync((double)packet.Data);
                            break;
                        case PDataType.ISO:
                            await CameraList[i].SetIsoSpeedAsync(int.Parse(packet.Data.ToString()));
                            break;
                        case PDataType.ShutterSpeed:
                            await CameraList[i].SetShutterSpeedAsync(new ShutterSpeed((string)packet.Data));
                            break;
                    }
                    Console.WriteLine($"[Command Thread]({i}) Sucessful execution.");
                }
                catch (Exception e)
                {
                    // The setting is incorect
                    if (i == 0)
                    {
                        Console.WriteLine($"[Command Thread]({i}) Invalid setting: " + e.Message);
                        break;
                    }
                    else
                    {
                        // Check if the camera is disconnected
                        if (packet.SettingType == PDataType.Picture)
                        {
                            Console.WriteLine($"[Command Thread] {CameraList[i].Name} disconnected...");
                            CameraList.RemoveAt(i);
                        }
                        Console.WriteLine($"[Command Thread] Command is not supported by {CameraList[i].Name}");
                    }

                }
            }
        }
    }
    /// <summary>
    /// Equality comparer for Camera
    /// </summary>
    class CameraCompare : IEqualityComparer<Camera>
    {
        public bool Equals(Camera x, Camera y)
        {
            return x.Name == y.Name;
        }

        public int GetHashCode(Camera obj)
        {
            int hashCode = 0;

            foreach (char chr in obj.Name)
                hashCode += chr;

            foreach (char chr in obj.Port)
                hashCode += chr;

            return hashCode;
        }
    }
}
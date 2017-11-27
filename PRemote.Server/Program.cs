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
        static bool IsStarted = true;
        static IPEndPoint BroadcastIP { get; } = new IPEndPoint(IPAddress.Broadcast, PConnection.UDPPort);
        static List<Camera> CameraList = new List<Camera>();
        static NetworkStream networkStream;

        static void Main(string[] args) // Main Method
        {
            Console.WriteLine("[PRemote] Starting PRemote server...");

            // Getting Cameras
            Thread cameraThread = new Thread(CameraThread);
            cameraThread.Start();
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
                // Delete disconnected Camera & Camera that can't be remote controlled
                for (int i = 0; i < CameraList.Count; i++)
                {
                    // Todo fix it, it don't work
                    if (!CameraList[i].CanCaptureImages)
                    {
                        Console.WriteLine($"[Camera Thread] Removed {CameraList[i].Name}");
                        CameraList.RemoveAt(i);
                    }
                }

                foreach (Camera camera in await Camera.GetCamerasAsync())
                {
                    // If Camera isn't in the list, add it
                    if (!CameraList.Contains(camera, new CameraCompare()))
                    {
                        Console.WriteLine($"[Camera Thread] Added {camera.Name}");
                        CameraList.Add(camera);
                    }
                }

                Thread.Sleep(5000);
            }

        }

        static void BroadcastThread() // Send broadcast packets through UDP
        {
            UdpClient udpClient = new UdpClient(0);
            Console.WriteLine("[UDP Thread] Sending broadcast packets...");

            while (IsStarted)
            {
                udpClient.Send(PConnection.UDPPacketData, PConnection.UDPPacketDataLenght, BroadcastIP);
                Thread.Sleep(2000);
            }
        }

        static void ConnectionThread() // Wait TCP connection from a client
        {
            IPEndPoint iP = new IPEndPoint(IPAddress.Any, PConnection.TCPPort);
            TcpListener tcpListener = new TcpListener(iP);
            tcpListener.Start();
            Console.WriteLine("[TCP Thread]Listening on " + iP.ToString());

            while (IsStarted)
            {
                Console.WriteLine("[TCP Thread] Waiting client...");

                TcpClient client = tcpListener.AcceptTcpClient();

                Console.WriteLine("[TCP Thread] Accepted conneciton from " + client.Client.RemoteEndPoint.ToString());

                // Send capabilities to Client
                Console.WriteLine("[TCP Thread] Sending capabilities...");
                byte[] data = MessagePackSerializer.Serialize(CameraList[0].GetCameraCapabilities());

                // Send in Stream
                networkStream = client.GetStream();
                Console.WriteLine($"[TCP Thread] Sending {data.Length} bytes");

                // Send bytes
                networkStream.Write(data, 0, data.Length);

                Console.WriteLine("[TCP Thread] Capabilities sent");

                // Start Transfer Thread
                Thread dataThread = new Thread(TransferThread);
                dataThread.Start();
            }
        }

        static async void TransferThread() // Receive data from client
        {
            Console.WriteLine("[Client Thread] Receiving data from client.");

            byte[] buffer = new byte[PConnection.BufferSize];

            while (IsStarted)
            {
                try
                {
                    // Read data
                    int received = networkStream.Read(buffer, 0, buffer.Length);
                    Console.WriteLine($"[Client Thread] Received {received} bytes.");

                    PPacket packet = MessagePackSerializer.Deserialize<PPacket>(buffer.SubArray(0, received));
                    Console.WriteLine("[Client Thread] Packet: " + packet.ToString());
                    await SetSetting(packet);
                }
                catch (Exception e)
                {
                    Console.WriteLine("[Client Thread] Received invalid data " + e.Message);
                }
            }
        }

        static async Task SetSetting(PPacket packet)
        {
            // Read a setting from a packet and try to set it
            try
            {
                switch (packet.SettingType)
                {
                    case PDataType.Aperture:
                        foreach (Camera camera in CameraList)
                            await camera.SetApertureAsync((double)packet.Data);
                        break;
                    case PDataType.ISO:
                        foreach (Camera camera in CameraList)
                            await camera.SetIsoSpeedAsync(int.Parse(packet.Data.ToString()));
                        break;
                    case PDataType.Picture:
                        foreach (Camera camera in CameraList)
                            await camera.CaptureImageAsync();
                        break;
                    case PDataType.ShutterSpeed:
                        foreach (Camera camera in CameraList)
                            await camera.SetShutterSpeedAsync(new ShutterSpeed((string)packet.Data));
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[Client Thread] Invalid setting " + e.Message);
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
            string hashCode = "";

            foreach (char chr in obj.Name)
                hashCode += (int)chr;

            foreach (char chr in obj.Port)
                hashCode += (int)chr;

            return int.Parse(hashCode);
        }
    }
}
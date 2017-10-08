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

namespace PRemote.Server
{
    class Program
    {
        //? Fields
        static bool IsStarted = true;
        static IPEndPoint BroadcastIP { get; } = new IPEndPoint(IPAddress.Broadcast, PConnection.UDPPort);
        static List<Camera> CameraList = new List<Camera>();

        static void Main(string[] args) //! Main Method
        {
            Console.WriteLine("[PRemote] Starting PRemote server...");

            // Getting Cameras
            Console.WriteLine("Listening for remote DSLR");
            Thread cameraThread = new Thread(CameraThread);
            cameraThread.Start();

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

        static async void CameraThread() //? Search for camera
        {
            while (IsStarted)
            {
                // Delete disconnected Camera & Camera that can't be remote controlled
                for (int i = 0; i < CameraList.Count; i++)
                    if (!CameraList[i].CanCaptureImages)
                        CameraList.RemoveAt(i);

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

        static void BroadcastThread() //? Send broadcast packets through UDP
        {
            UdpClient udpClient = new UdpClient(0);
            Console.WriteLine("[UDP Thread] Sending broadcast packets...");

            while (IsStarted)
            {
                udpClient.Send(PConnection.UDPPacketData, PConnection.UDPPacketDataLenght, BroadcastIP);
                Thread.Sleep(2000);
            }
        }

        static void ConnectionThread() //? Wait TCP connection from a client
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Any, PConnection.TCPPort);

            while (IsStarted)
            {
                Console.WriteLine("[TCP Thread] Waiting client...");

                TcpClient client = tcpListener.AcceptTcpClient();

                Console.WriteLine("[TCP Thread] Accepted conneciton from " + client.Client.RemoteEndPoint.ToString());

                // Send capabilities to Client
                Stream stream = new MemoryStream();
                MessagePackSerializer.Serialize(stream, new PPacket(PDataType.Configuration, (CameraList[0].GetCameraCapabilities())));
                long lenght = stream.Length;
                byte[] buffer = new byte[PConnection.BufferSize];

                using (NetworkStream networkStream = client.GetStream())
                {
                    // Send size to client
                    networkStream.Write(BitConverter.GetBytes(lenght), 0, 8);

                    // Send bytes
                    while (stream.Position <= lenght)
                    {
                        //? Get left bytes to send
                        int leftBytes = (int)(lenght - stream.Position);
                        if (leftBytes > PConnection.BufferSize)
                            leftBytes = PConnection.BufferSize;

                        stream.Read(buffer, 0, leftBytes);
                        networkStream.Write(buffer, 0, leftBytes);
                    }
                }
                stream.Close();

                // Start Transfer Thread
                Thread dataThread = new Thread(TransferThread);
                dataThread.Start(client);
            }
        }

        static async void TransferThread(object obj) //? Receive data from client
        {
            Console.WriteLine("[Client Thread] Receiving data from client.");

            TcpClient client = (TcpClient)obj;
            NetworkStream networkStream = client.GetStream();
            MemoryStream packetStream = new MemoryStream();
            byte[] buffer = new byte[PConnection.BufferSize];

            while (IsStarted)
            {
                try
                {
                    // Read lenght
                    networkStream.Read(buffer, 0, 8);
                    long lenght = BitConverter.ToInt32(buffer, 0);
                    int read = 0;

                    // Read data
                    while (packetStream.Length < lenght)
                    {
                        read = networkStream.Read(buffer, 0, buffer.Length);
                        packetStream.Write(buffer, 0, read);
                    }

                    PPacket packet = MessagePackSerializer.Deserialize<PPacket>(packetStream);
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
            //? Read a setting from a packet and try to set it
            try
            {
                switch (packet.Settingtype)
                {
                    case PDataType.Aperture:
                        foreach (Camera camera in CameraList)
                            await camera.SetApertureAsync((double)packet.Data);
                        break;
                    case PDataType.ISO:
                        foreach (Camera camera in CameraList)
                            await camera.SetIsoSpeedAsync((int)packet.Data);
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

    class CameraCompare : IEqualityComparer<Camera> //! Equality comparer for Camera
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

            return int.Parse(hashCode);
        }
    }
}

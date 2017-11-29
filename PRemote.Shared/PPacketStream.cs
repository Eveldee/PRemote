using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

using MessagePack;
using System.Threading.Tasks;
using System.Linq;

namespace PRemote.Shared
{
    /// <summary>
    /// Used to send <see cref="PPacket"/> through <see cref="System.Net.Sockets.NetworkStream"/>
    /// </summary>
    public class PPacketStream : INetworkStreamManager<PPacket>
    {
        /// <summary>
        /// The used <see cref="System.Net.Sockets.NetworkStream"/>
        /// </summary>
        public NetworkStream NetworkStream { private set; get; }

        /// <summary>
        /// Construct from an existing stream
        /// </summary>
        /// <param name="networkStream">A connected <see cref="System.Net.Sockets.NetworkStream"/></param>
        public PPacketStream(NetworkStream networkStream) => NetworkStream = networkStream;

        /// <summary>
        /// Receive one <see cref="PPacket"/>
        /// </summary>
        /// <returns></returns>
        public PPacket Receive()
        {
            // Declarations
            int size;
            int position = 0;
            byte[] sizeBuffer = new byte[4];

            // Read lenght
            NetworkStream.Read(sizeBuffer, 0, 4);
            size = BitConverter.ToInt32(sizeBuffer, 0);

            byte[] data = new byte[size];

            // Read data
            while (position < size)
            {
                int leftBytes = size - position;
                if (leftBytes > PConnection.BufferSize)
                    leftBytes = PConnection.BufferSize;

                int receivedBytes = NetworkStream.Read(data, position, leftBytes);

                position += receivedBytes;
            }

            return MessagePackSerializer.Deserialize<PPacket>(data);
        }
        /// <summary>
        /// Receive multiples <see cref="PPacket"/>
        /// </summary>
        /// <param name="number">The number of <see cref="PPacket"/> to receive</param>
        /// <exception cref="ArgumentOutOfRangeException" />
        /// <returns></returns>
        public PPacket[] Receive(int number)
        {
            if (number < 1)
            {
                throw new ArgumentOutOfRangeException("number", "can't receive less than 1 PPacket");
            }

            PPacket[] arr = new PPacket[number];

            for (int i = 0; i < number; i++)
            {
                arr[i] = Receive();
            }

            return arr;
        }

        /// <summary>
        /// Receive one <see cref="PPacket"/> async
        /// </summary>
        /// <returns></returns>
        public async Task<PPacket> ReceiveAsync()
        {
            // Declarations
            int size;
            int position = 0;
            byte[] sizeBuffer = new byte[4];

            // Read lenght
            await NetworkStream.ReadAsync(sizeBuffer, 0, 4);
            size = BitConverter.ToInt32(sizeBuffer, 0);

            byte[] data = new byte[size];

            // Read data
            while (position < size)
            {
                int leftBytes = size - position;
                if (leftBytes > PConnection.BufferSize)
                    leftBytes = PConnection.BufferSize;

                await NetworkStream.ReadAsync(data, position, leftBytes);

                position += leftBytes;
            }

            return MessagePackSerializer.Deserialize<PPacket>(data);
        }
        /// <summary>
        /// Receive multiples <see cref="PPacket"/> async
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException" />
        /// <param name="number">The number of <see cref="PPacket"/> to receive</param>
        /// <returns></returns>
        public async Task<PPacket[]> ReceiveAsync(int number)
        {
            if (number < 1)
            {
                throw new ArgumentOutOfRangeException("number", "can't receive less than 1 PPacket");
            }

            PPacket[] arr = new PPacket[number];

            for (int i = 0; i < number; i++)
            {
                arr[i] = await ReceiveAsync();
            }

            return arr;
        }

        /// <summary>
        /// Send one <see cref="PPacket"/>
        /// </summary>
        /// <param name="obj">The <see cref="PPacket"/> to send</param>
        public void Send(PPacket obj)
        {
            // Declarations
            int size;
            int position = 0;

            // Serialize
            byte[] data = MessagePackSerializer.Serialize(obj);

            // Send lenght
            size = data.Length;
            byte[] sizeBuffer = BitConverter.GetBytes(size);
            NetworkStream.Write(sizeBuffer, 0, 4);

            // Send data
            while (position < size)
            {
                int leftBytes = size - position;
                if (leftBytes > PConnection.BufferSize)
                    leftBytes = PConnection.BufferSize;

                NetworkStream.Write(data, position, leftBytes);

                position += leftBytes;
            }
        }
        /// <summary>
        /// Send multiples <see cref="PPacket"/>
        /// </summary>
        /// <param name="obj">The <see cref="PPacket"/> to send</param>
        public void Send(params PPacket[] obj)
        {
            foreach (PPacket packet in obj)
            {
                Send(packet);
            }
        }

        /// <summary>
        /// Send one <see cref="PPacket"/> async
        /// </summary>
        /// <param name="obj">The <see cref="PPacket"/> to send</param>
        /// <returns></returns>
        public async Task SendAsync(PPacket obj)
        {
            // Declarations
            int size;
            int position = 0;

            // Serialize
            byte[] data = MessagePackSerializer.Serialize(obj);

            // Send lenght
            size = data.Length;
            byte[] sizeBuffer = BitConverter.GetBytes(size);
            await NetworkStream.WriteAsync(sizeBuffer, 0, 4);

            // Send data
            while (position < size)
            {
                int leftBytes = size - position;
                if (leftBytes > PConnection.BufferSize)
                    leftBytes = PConnection.BufferSize;

                await NetworkStream.WriteAsync(data, position, leftBytes);

                position += leftBytes;
            }
        }
        /// <summary>
        /// Send multiple <see cref="PPacket"/> async
        /// </summary>
        /// <param name="obj">The <see cref="PPacket"/> to send</param>
        /// <returns></returns>
        public async Task SendAsync(params PPacket[] obj)
        {
            foreach(PPacket packet in obj)
            {
                await SendAsync(packet);
            }
        }
    }
}

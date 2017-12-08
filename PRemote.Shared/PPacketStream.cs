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
    /// Used to send <see cref="object"/> through <see cref="System.Net.Sockets.NetworkStream"/>
    /// </summary>
    public class PacketStream : INetworkStreamManager
    {
        /// <summary>
        /// The used <see cref="System.Net.Sockets.NetworkStream"/>
        /// </summary>
        public NetworkStream NetworkStream { private set; get; }

        /// <summary>
        /// Construct from an existing stream
        /// </summary>
        /// <param name="networkStream">A connected <see cref="System.Net.Sockets.NetworkStream"/></param>
        public PacketStream(NetworkStream networkStream) => NetworkStream = networkStream;

        /// <summary>
        /// Receive one <see cref="object"/>
        /// </summary>
        /// <returns></returns>
        public T Receive<T>()
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

            return MessagePackSerializer.Deserialize<T>(data);
        }
        /// <summary>
        /// Receive multiples <see cref="object"/>
        /// </summary>
        /// <param name="number">The number of <see cref="object"/> to receive</param>
        /// <exception cref="ArgumentOutOfRangeException" />
        /// <returns></returns>
        public T[] Receive<T>(int number)
        {
            if (number < 1)
            {
                throw new ArgumentOutOfRangeException("number", "can't receive less than 1 T");
            }

            T[] arr = new T[number];

            for (int i = 0; i < number; i++)
            {
                arr[i] = Receive<T>();
            }

            return arr;
        }

        /// <summary>
        /// Receive one <see cref="object"/> async
        /// </summary>
        /// <returns></returns>
        public async Task<T> ReceiveAsync<T>()
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

            return MessagePackSerializer.Deserialize<T>(data);
        }
        /// <summary>
        /// Receive multiples <see cref="object"/> async
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException" />
        /// <param name="number">The number of <see cref="object"/> to receive</param>
        /// <returns></returns>
        public async Task<T[]> ReceiveAsync<T>(int number)
        {
            if (number < 1)
            {
                throw new ArgumentOutOfRangeException("number", "can't receive less than 1 T");
            }

            T[] arr = new T[number];

            for (int i = 0; i < number; i++)
            {
                arr[i] = await ReceiveAsync<T>();
            }

            return arr;
        }

        /// <summary>
        /// Send one <see cref="object"/>
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to send</param>
        public void Send<T>(T obj)
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
        /// Send multiples <see cref="object"/>
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to send</param>
        public void Send<T>(params T[] obj)
        {
            foreach (T packet in obj)
            {
                Send(packet);
            }
        }

        /// <summary>
        /// Send one <see cref="object"/> async
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to send</param>
        /// <returns></returns>
        public async Task SendAsync<T>(T obj)
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
        /// Send multiples <see cref="object"/> async
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to send</param>
        /// <returns></returns>
        public async Task SendAsync<T>(params T[] obj)
        {
            foreach(T packet in obj)
            {
                await SendAsync(packet);
            }
        }
    }
}

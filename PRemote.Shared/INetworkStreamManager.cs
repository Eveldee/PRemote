using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PRemote.Shared
{
    /// <summary>
    /// Manage a NetworkStream
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface INetworkStreamManager<T>
    {
        NetworkStream NetworkStream { get; }

        /// <summary>
        /// Receive an object
        /// </summary>
        /// <returns></returns>
        T Receive();
        /// <summary>
        /// Receive an object async
        /// </summary>
        /// <returns></returns>
        Task<T> ReceiveAsync();

        /// <summary>
        /// Receive multiple object
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        T[] Receive(int number);
        /// <summary>
        /// Receive multiple object async
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        Task<T[]> ReceiveAsync(int number);

        /// <summary>
        /// Send an object
        /// </summary>
        /// <param name="obj"></param>
        void Send(T obj);
        /// <summary>
        /// Send an object async
        /// </summary>
        /// <param name="obj"></param>
        Task SendAsync(T obj);

        /// <summary>
        /// Send multiple object
        /// </summary>
        /// <param name="obj"></param>
        void Send(params T[] obj);
        /// <summary>
        /// Send multiple object async
        /// </summary>
        /// <param name="obj"></param>
        Task SendAsync(params T[] obj);
    }
}

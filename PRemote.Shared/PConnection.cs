using System;
using MessagePack;

namespace PRemote.Shared
{
    /// <summary>
    /// Store some const fields.
    /// </summary>
    public class PConnection
    {
        public const int UDPPort = 6548;
        public const int TCPPort = 6549;

        public static byte[] UDPPacketData { get; } = { 0, 255, 155, 143 };
        public static int UDPPacketDataLenght => UDPPacketData.Length;

        public const int BufferSize = 1024;
    }
}

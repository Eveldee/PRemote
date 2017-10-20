using System;
using MessagePack;

namespace PRemote.Shared
{
    public class PConnection //! Store some coonst fields
    {
        public const int UDPPort = 6548;
        public const int TCPPort = 6549;

        public static readonly byte[] UDPPacketData = { 0, 255, 155, 143 };
        public static readonly int UDPPacketDataLenght = UDPPacketData.Length;

        public const int BufferSize = 1024;
    }

    [MessagePackObject]
    public class PPacket //! Main class for TCP data
    {
        // Setting Type
        [Key(0)]
        public PDataType SettingType { get; }

        // Data
        [Key(1)]
        public object Data { get; }

        // Constructor
        public PPacket(PDataType settingType, object data)
        {
            SettingType = settingType;
            Data = data;
        }

        public override string ToString()
        {
            return SettingType.ToString() + "=" + Data.ToString();
        }
    }

    //! The type of the setting
    public enum PDataType { Picture, ISO, ShutterSpeed, Aperture, Battery }
}

using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace PRemote.Shared
{
    /// <summary>
    /// Main class for TCP data
    /// </summary>
    [MessagePackObject]
    public class PPacket
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

    /// <summary>
    /// The type of the setting
    /// </summary>
    public enum PDataType { Picture, ISO, ShutterSpeed, Aperture, Battery, Configuration }
}

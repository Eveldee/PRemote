using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PRemote.Shared
{
    /// <summary>
    /// Represent capabilities of a camera
    /// </summary>
    [MessagePackObject]
    public class CameraCapabilities
    {
        [Key(0)]
        public string Name { get; }
        [Key(1)]
        public string BatteryLevel { get; }

        [Key(2)]
        public bool CanBeConfigured { get; }
        [Key(3)]
        public bool CanCapturePreviews { get; }
        [IgnoreMember]
        public bool CanSetAperture
        {
            get { return SupportedApertures.Count() > 0 ? true : false; }
        }
        [IgnoreMember]
        public bool CanSetIsoSpeed
        {
            get { return SupportedIsoSpeeds.Count() > 0 ? true : false; }
        }
        [IgnoreMember]
        public bool CanSetShutterSpeed
        {
            get { return SupportedShutterSpeeds.Count() > 0 ? true : false; }
        }

        [Key(4)]
        public double CurrentAperture { get; }
        [Key(5)]
        public int CurrentIsoSpeed { get; }
        [Key(6)]
        public string CurrentShutterSpeed { get; }

        [Key(7)]
        public double[] SupportedApertures { get; }
        [Key(8)]
        public int[] SupportedIsoSpeeds { get; }
        [Key(9)]
        public string[] SupportedShutterSpeeds { get; }

        // Constructor
        public CameraCapabilities(string name, string battery, bool configurable, bool canPreview,
            double aperture, int isoSpeed, string shutterSpeed,
            double[] apertures, int[] isoSpeeds, string[] shutterSpeeds)
        {
            Name = name;
            BatteryLevel = battery;

            CanBeConfigured = configurable;
            CanCapturePreviews = canPreview;

            CurrentAperture = aperture;
            CurrentIsoSpeed = isoSpeed;
            CurrentShutterSpeed = shutterSpeed;

            SupportedApertures = apertures;
            SupportedIsoSpeeds = isoSpeeds;
            SupportedShutterSpeeds = shutterSpeeds;
        }

        //public static CameraCapabilities FromObjectArray(object[] capabilities)
        //{
        //    return new CameraCapabilities((string)capabilities[0], (string)capabilities[1], (bool)capabilities[2], (bool)capabilities[3],
        //        (double)capabilities[4], (int)capabilities[5], (string)capabilities[6],
        //        (double[])capabilities[7], (int[])capabilities[8], (string[])capabilities[9]);
        //}

    }
}

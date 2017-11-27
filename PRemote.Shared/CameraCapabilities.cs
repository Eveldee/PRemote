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
    public struct CameraCapabilities
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
        public IEnumerable<double> SupportedApertures { get; }
        [Key(5)]
        public IEnumerable<int> SupportedIsoSpeeds { get; }
        [Key(6)]
        public IEnumerable<string> SupportedShutterSpeeds { get; }

        // Constructor
        public CameraCapabilities(string name, string battery, bool configurable, bool canPreview,
            IEnumerable<double> apertures, IEnumerable<int> isoSpeeds, IEnumerable<string> shutterSpeeds)
        {
            Name = name;
            BatteryLevel = battery;

            CanBeConfigured = configurable;
            CanCapturePreviews = canPreview;

            SupportedApertures = apertures;
            SupportedIsoSpeeds = isoSpeeds;
            SupportedShutterSpeeds = shutterSpeeds;
        }

    }
}

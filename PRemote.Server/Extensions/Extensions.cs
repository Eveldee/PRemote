using System;
using System.Collections.Generic;
using System.Text;
using System.Devices;
using System.Linq;

using PRemote.Shared;
using System.Threading.Tasks;

namespace PRemote.Server.Extensions
{
    public static class Extensions // Main extensions class
    {
        /// <summary>
        /// Extensions to get Capabilities easier
        /// </summary>
        /// <remarks>
        /// This code is so hugeee and bad-optimized, but there's no
        /// other way to do except using Reflection.
        /// </remarks>
        /// <param name="camera">A valid camera</param>
        /// <returns></returns>
        public async static Task<CameraCapabilities> GetCameraCapabilitiesAsync(this Camera camera)
        {
            try
            {
                // Ini fields
                string name;
                string batteryLevel;
                bool canBeConfigured;
                bool canCapturePreview;

                double aperture;
                int iso;
                string shutterSpeed;

                double[] supportedApertures;
                int[] supportedIso;
                string[] supportedShutterSpeeds;

                // Get all those things
                try
                {
                    name = camera.Name;
                }
                catch (Exception)
                {
                    name = "N/A";
                }
                try
                {
                    batteryLevel = await camera.GetBatteryLevelAsync();
                }
                catch (Exception)
                {
                    batteryLevel = "N/A";
                }
                try
                {
                    canBeConfigured = camera.CanBeConfigured;
                }
                catch (Exception)
                {
                    canBeConfigured = false;
                }
                try
                {
                    canCapturePreview = camera.CanCapturePreviews;
                }
                catch (Exception)
                {
                    canCapturePreview = false;
                }
                try
                {
                    aperture = await camera.GetApertureAsync();
                }
                catch (Exception)
                {
                    aperture = -1;
                }
                try
                {
                    iso = await camera.GetIsoSpeedAsync();
                }
                catch (Exception)
                {
                    iso = -1;
                }
                try
                {
                    shutterSpeed = (await camera.GetShutterSpeedAsync()).TextualRepresentation;
                }
                catch (Exception)
                {
                    shutterSpeed = "N/A";
                }
                try
                {
                    supportedApertures = (await camera.GetSupportedAperturesAsync()).ToArray();
                }
                catch (Exception)
                {
                    supportedApertures = Array.Empty<double>();
                }
                try
                {
                    supportedIso = (await camera.GetSupportedIsoSpeedsAsync()).ToArray();
                }
                catch (Exception)
                {
                    supportedIso = Array.Empty<int>();
                }
                try
                {
                    supportedShutterSpeeds = (await camera.GetSupportedShutterSpeedsAsync()).Select(x => x.TextualRepresentation).ToArray();
                }
                catch (Exception)
                {
                    supportedShutterSpeeds = Array.Empty<string>();
                }

                // Return.
                return new CameraCapabilities(name, batteryLevel, canBeConfigured, canCapturePreview,
                    aperture, iso, shutterSpeed,
                    supportedApertures, supportedIso, supportedShutterSpeeds);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " " + e.StackTrace);
                return default(CameraCapabilities);
            }
        }
    }
}

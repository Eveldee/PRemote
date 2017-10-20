using System;
using System.Collections.Generic;
using System.Text;
using System.Devices;
using System.Linq;

using PRemote.Shared;
using System.Threading.Tasks;

namespace PRemote.Server.Extensions
{
    public static class Extensions //! Main extensions class
    {
        //? Extensions to get Capabilities easier
        public static CameraCapabilities GetCameraCapabilities(this Camera camera)
        {
            try
            {
                //				return new CameraCapabilities(camera.Name, await camera.GetBatteryLevelAsync(), camera.CanBeConfigured, camera.CanCapturePreviews,
                //					await camera.GetSupportedAperturesAsync(), await camera.GetSupportedIsoSpeedsAsync(), (await camera.GetSupportedShutterSpeedsAsync()).Select(x => x.TextualRepresentation));
                return new CameraCapabilities(camera.Name, null, camera.CanBeConfigured, camera.CanCapturePreviews,
                    null, null, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " " + e.StackTrace);
                return default(CameraCapabilities);
            }
        }
    }
}

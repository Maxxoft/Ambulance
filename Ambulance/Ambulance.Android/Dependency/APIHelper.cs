using System;
using Ambulance.Dependency;
using Ambulance.Droid;
using Ambulance.ObjectModel;
using Android.Media;
using Android.Content.Res;
using System.Collections.Generic;
using System.Threading.Tasks;

[assembly: Xamarin.Forms.Dependency(typeof(APIHelper))]
namespace Ambulance.Droid
{
	public class APIHelper: IAPIHelper
	{
        public void ClearCookies()
        {
            throw new NotImplementedException();
        }

        public void CloseApp()
		{
			Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
		}

        public string GetAppVersion()
        {
            return MainActivity.AppVersion;
        }

        public GeoLocation GetCurrentLocation()
        {
            return MainActivity.GetCurrentLocation();
        }

        public string GetDeviceVersion()
        {
            throw new NotImplementedException();
        }

        public GeoLocation GetLastKnownLocation()
		{
			return MainActivity.GetCurrentLocation();
		}

        public List<string> GetLocationProviders()
        {
            throw new NotImplementedException();
        }

        public void PlayAlertSound()
        {
            MainActivity mainActivity = Xamarin.Forms.Forms.Context as MainActivity;
            AssetFileDescriptor afd = mainActivity.Assets.OpenFd("Alert.mp3");
			var player = new MediaPlayer();
            player.SetDataSource(afd.FileDescriptor, afd.StartOffset, afd.Length);
			player.Prepare();
			player.Start();           
        }

        public void RequestLocation(string provider = "")
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RequestLocationsPermissions()
		{
			MainActivity.RequestLocationPermissions();
            return true;
		}

        public void StartRequestLocation(string provider = "")
        {
            throw new NotImplementedException();
        }

        public void StopRequestLocation()
        {
            throw new NotImplementedException();
        }
        
    }
}

using Ambulance.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ambulance.Dependency
{
	public interface IAPIHelper
	{
		void CloseApp();

		Task<bool> RequestLocationsPermissions();
        List<string> GetLocationProviders();
        void StartRequestLocation(string provider = "");
        void StopRequestLocation();
		GeoLocation GetCurrentLocation();
        GeoLocation GetLastKnownLocation();
        void PlayAlertSound();

        string GetAppVersion();
        string GetDeviceVersion();

        void ClearCookies();
	}
}

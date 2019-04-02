using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Locations;
using Ambulance.Dependency;
using Plugin.CurrentActivity;
using System.Net;
using Plugin.Permissions;
using Android.Content;
using Ambulance.ObjectModel;

namespace Ambulance.Droid
{
    [Activity(Label = "Ambulance", Icon = "@drawable/app_icon", Theme = "@style/MyTheme",
        MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Landscape)]

    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static MainActivity Instance { get; private set; }
        public static string AppVersion { get; private set; }

        private static LocationManager LocManager;
        //private static AssetManager AccesMgr;

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            var metrics = Resources.DisplayMetrics;
            DependencyData.Height = ConvertPixelsToDp(metrics.HeightPixels);
            DependencyData.Widht = ConvertPixelsToDp(metrics.WidthPixels);
            //IsPlayServicesAvailable();
            try
            {
                LocManager = (LocationManager)GetSystemService(LocationService);
                //TelephonyManager mTelephonyMgr = (TelephonyManager)GetSystemService(TelephonyService);
                //StaticDeviceInfo.PhoneNumber = mTelephonyMgr.Line1Number;				
            }
            catch
            {
            }

            Rg.Plugins.Popup.Popup.Init(this, bundle);
            global::Xamarin.Forms.Forms.Init(this, bundle);
            Instance = this;

            CrossCurrentActivity.Current.Init(this, bundle);
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            AppVersion = ApplicationContext.PackageManager.GetPackageInfo(ApplicationContext.PackageName, 0).VersionName;

            try
            {
                Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);
            }
            catch { }
            LoadApplication(new App());
        }

        private int ConvertPixelsToDp(float pixelValue)
        {
            var dp = (int)((pixelValue) / Resources.DisplayMetrics.Density);
            return dp;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public void SetNotificationSetting()
        {
            var alert = GetAlert(
              "Пуш-уведомления",
              "Желаете настроить получение пуш-уведомлений сейчас?");
            alert.SetPositiveButton(
                "Настройки",
                (sender, e) =>
                {
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                    {
                        Intent intent = new Intent(Android.Provider.Settings.ActionApplicationDetailsSettings);
                        intent.SetData(Android.Net.Uri.Parse("package:" + this.PackageName));
                        intent.AddCategory(Intent.CategoryDefault);
                        StartActivity(intent);
                    }
                });
            alert.SetNegativeButton("Закрыть", (sender, e) => { });
            alert.Show();
        }

        private Android.App.AlertDialog.Builder GetAlert(string title, string message)
        {

            Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
            alert.SetTitle(title);
            alert.SetMessage(message);

            return alert;
        }

        /*public static Location GetCurrentLocation()
        {			
            Location res = null;
            try
            {
                //if (LocManager == null) LocManager = (LocationManager)GetSystemService(LocationService);
                res = LocManager.GetLastKnownLocation(LocationManager.NetworkProvider);
            }
            catch (Exception e)
            {
				res = new Location("")
				{
					Latitude = 55.765182,
					Longitude = 37.591689
				};               
            }
            return res;
        }*/

        static readonly string[] PermissionsLocation =
        {
            Android.Manifest.Permission.AccessCoarseLocation,
            Android.Manifest.Permission.AccessFineLocation
        };
        static readonly int RequestLocationId = 0;

        /*async Task GetLocationPermissionAsync()
		{
			//Check to see if any permission in our group is available, if one, then all are
			const string permission = Android.Manifest.Permission.AccessFineLocation;
			if (CheckSelfPermission(permission) == (int)Permission.Granted)
			{
				await GetLocationAsync();
				return;
			}

			//need to request permission
			if (ShouldShowRequestPermissionRationale(permission))
			{
				//Explain to the user why we need to read the contacts
				Snackbar.Make(layout, "Location access is required to show coffee shops nearby.", Snackbar.LengthIndefinite)
						.SetAction("OK", v => RequestPermissions(PermissionsLocation, RequestLocationId))
						.Show();
				return;
			}
			//Finally request permissions with the list of permissions and Id
			RequestPermissions(PermissionsLocation, RequestLocationId);
		}*/

        public static void RequestLocationPermissions()
        {
            if ((int)Build.VERSION.SdkInt >= 23)
            {
                if (Instance?.CheckSelfPermission(Android.Manifest.Permission.AccessCoarseLocation) != (int)Permission.Granted ||
                    Instance?.CheckSelfPermission(Android.Manifest.Permission.AccessFineLocation) != (int)Permission.Granted)
                {
                    Instance?.RequestPermissions(PermissionsLocation, RequestLocationId);
                }
            }
        }

        public static GeoLocation GetCurrentLocation()
        {
            LocManager = (LocationManager)Instance.GetSystemService(Context.LocationService);

            GeoLocation res = new GeoLocation();
            res.Error = string.Empty;

            Location loc = null;
            try
            {
                loc = LocManager?.GetLastKnownLocation(LocationManager.NetworkProvider);
            }
            catch (Exception ex)
            {
                res.Error = "Network: " + ex.Message;
            }

            if (loc == null || Math.Abs(loc.Latitude) < 0.1 || Math.Abs(loc.Longitude) < 0.1)
            {
                try
                {
                    loc = LocManager?.GetLastKnownLocation(LocationManager.GpsProvider);
                }
                catch (Exception ex)
                {
                    if (string.IsNullOrEmpty(res.Error))
                        res.Error = "GPS: " + ex.Message;
                    else
                        res.Error = res.Error + " && GPS: " + ex.Message;
                }
            }

#if DEBUG
            //return new GeoLocation { Latitude = 55.71, Longitude = 37.62 };
            //res.Latitude = loc?.Latitude ?? 66.1224374113707;
            //res.Longitude = loc?.Longitude ?? 76.6590342846295;
#endif
            res.Latitude = loc?.Latitude ?? 0;
			res.Longitude = loc?.Longitude ?? 0;

            return res;
        }

        private static bool CertificateValidationCallBack(
             object sender,
             System.Security.Cryptography.X509Certificates.X509Certificate certificate,
             System.Security.Cryptography.X509Certificates.X509Chain chain,
             System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            // If the certificate is a valid, signed certificate, return true.
            if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
            {
                return true;
            }

            // If there are errors in the certificate chain, look at each error to determine the cause.
            if ((sslPolicyErrors & System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors) != 0)
            {
                if (chain != null && chain.ChainStatus != null)
                {
                    foreach (System.Security.Cryptography.X509Certificates.X509ChainStatus status in chain.ChainStatus)
                    {
                        if ((certificate.Subject == certificate.Issuer) &&
                           (status.Status == System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.UntrustedRoot))
                        {
                            // Self-signed certificates with an untrusted root are valid. 
                            continue;
                        }
                        else
                        {
                            if (status.Status != System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.NoError)
                            {
                                // If there are any other errors in the certificate chain, the certificate is invalid,
                                // so the method returns false.
                                return false;
                            }
                        }
                    }
                }

                // When processing reaches this line, the only errors in the certificate chain are 
                // untrusted root errors for self-signed certificates. These certificates are valid
                // for default Exchange server installations, so return true.
                return true;
            }
            else
            {
                // In all other cases, return false.
                return false;
            }
        }

        /*public void IsPlayServicesAvailable()
        {
            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (resultCode != ConnectionResult.Success)
            {
            }
        }*/
    }
}


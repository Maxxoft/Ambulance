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
using Android;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ambulance.Droid
{
    [Activity(Label = "Ambulance", Icon = "@drawable/app_icon", Theme = "@style/MyTheme",
        MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Landscape)]

    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, ILocationListener
    {
        public static MainActivity Instance { get; private set; }
        public static string AppVersion { get; private set; }

        LocationManager locManager;
        string locProvider;
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
                //LocManager = (LocationManager)GetSystemService(LocationService);
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

        #region LOCATIONS
        public int CurrentLocationAccuracy = int.MaxValue;
        public static GeoLocation curLocation = new GeoLocation
        {

#if DEBUG
            RequestAmount = 0,
            //Latitude = 55.75,
            //Longitude = 37.61
#else
            RequestAmount = 0,
			Latitude = 0,
			Longitude = 0
#endif
        };


        static readonly int LocationPermissions = 1;
        public bool? LocationPermissionsGranted, CameraPermissionsGranted, GalleryPermissionsGranted;

        public void RequestLocationPermissions()
        {
            if ((int)Build.VERSION.SdkInt >= 23)
            {
                if (CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) != (int)Android.Content.PM.Permission.Granted ||
                    CheckSelfPermission(Manifest.Permission.AccessFineLocation) != (int)Android.Content.PM.Permission.Granted)
                {
                    string[] perms =
                    {
                        Manifest.Permission.AccessCoarseLocation,
                        Manifest.Permission.AccessFineLocation
                    };
                    Instance?.RequestPermissions(perms, LocationPermissions);
                    return;
                }
                LocationPermissionsGranted = true;
                return;
            }
            LocationPermissionsGranted = true;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            if (requestCode == LocationPermissions)
            {
                LocationPermissionsGranted = grantResults != null && grantResults.Length > 0 && grantResults[0] == Android.Content.PM.Permission.Granted;
            }
        }

        public static List<string> GetLocationProviders()
        {
            Instance.locManager = (LocationManager)Instance.GetSystemService(LocationService);
            var providers = Instance.locManager.GetProviders(true);
            var res = new List<string>();
            foreach (var p in providers) res.Add(p);
            // gps / network / passive
            return res.OrderBy(x => x).ToList();
        }

        //ILocationListener locListener;
        public static async void RequestLocation(string provider)
        {
            if (Instance.locManager != null) return;
            //return;
            var locManager = (LocationManager)Instance.GetSystemService(LocationService);
            Instance.locManager = locManager;
            while (true)
            {
                try
                {
                    //    if (ContextCompat.CheckSelfPermission(Instance, Manifest.Permission.AccessFineLocation) != Permission.Granted)
                    //        ActivityCompat.RequestPermissions(Instance, new String[] { Manifest.Permission.AccessFineLocation }, 1);

                    if (Instance.locManager == null) break;
                    Instance.RunOnUiThread(() =>
                    {
                        try
                        {
                            locManager.RequestSingleUpdate(LocationManager.NetworkProvider, Instance, null);
                            locManager.RequestSingleUpdate(LocationManager.GpsProvider, Instance, null);
                            locManager.RequestSingleUpdate(LocationManager.PassiveProvider, Instance, null);
                        }
                        catch { }
                    });
                }
                catch (Exception ex)
                {

                }
                await Task.Delay(2000);
            }

            return;

        }

        public static void StopRequestLocation()
        {
            System.Diagnostics.Debug.WriteLine("StopRequestLocation: ");
            if (Instance?.locManager != null)
            {
                Instance.locManager.RemoveUpdates(Instance);
                Instance.locManager = null;
                //curLocation.Provider = string.Empty;
                //curLocation.RequestAmount = 0;
            }
        }

        public static GeoLocation GetCurrentLocation()
        {
            //System.Diagnostics.Debug.WriteLine("GetCurrentLocation: ");
            return curLocation;
        }

        public GeoLocation GetLastKnownLocation()
        {
            try
            {
                if (curLocation.RequestAmount > 0)
                    return curLocation;

                var locManager = (LocationManager)Instance.GetSystemService(LocationService);
                var res = locManager.GetLastKnownLocation(LocationManager.GpsProvider);
                var res2 = locManager.GetLastKnownLocation(LocationManager.NetworkProvider);
                var res3 = locManager.GetLastKnownLocation(LocationManager.PassiveProvider);
                if (res != null)
                    return new GeoLocation(res.Latitude, res.Longitude);
                else
                  if (res2 != null)
                    return new GeoLocation(res2.Latitude, res2.Longitude);
                else
                  if (res3 != null)
                    return new GeoLocation(res3.Latitude, res3.Longitude);
            }
            catch { }

            return curLocation;
        }

        public void OnLocationChanged(Location location)
        {
            System.Diagnostics.Debug.WriteLine("OnLocationChanged: " + Instance.locProvider);
            if (curLocation.RequestAmount == 0)
                curLocation.FirstResponseDate = DateTime.Now;
            curLocation.RequestAmount++;
            if (location.Accuracy < CurrentLocationAccuracy && location != null && location.Latitude > 0 && location.Longitude > 0)
            {
                curLocation.Latitude = location.Latitude;
                curLocation.Longitude = location.Longitude;
                curLocation.LastResponseDate = DateTime.Now;
            }
            /*  if (location != null && location.Latitude > 0 && location.Longitude > 0)
              {
                  curLocation.Latitude = location.Latitude;
                  curLocation.Longitude = location.Longitude;
                  curLocation.LastResponseDate = DateTime.Now;
              }*/
            else
                curLocation.Error = "Provider " + curLocation.Provider ?? "" + " returned nothing";
        }

        public void OnProviderDisabled(string provider)
        {
            if (locProvider == provider)
                locManager?.RemoveUpdates(this);
        }

        public void OnProviderEnabled(string provider)
        {
            //if (locProvider == provider)
            //    locManager?.RequestLocationUpdates(locProvider, 0, 0, this);
        }

        #endregion

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

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
            //
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


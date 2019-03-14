using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Net;
using Android.Content.PM;
using Ambulance.Droid.Dependency;
using Android.Util;
using Java.Security;
using Java.Security.Spec;
using System;
using Ambulance.Dependency;
using Ambulance.ObjectModel;

[assembly: Xamarin.Forms.Dependency(typeof(NavigatorHelper))]
namespace Ambulance.Droid.Dependency
{
    public class NavigatorHelper : INavigatorHelper
    {
        private const string Key = @"MIIBVQIBADANBgkqhkiG9w0BAQEFAASCAT8wggE7AgEAAkEAuQKgUq+gbytc8Lk/" +
"3ijMYBBtKhvmJvn7LC23fWyyVCoSUmPtHDMOB9uQyEgBeN1ciL8S3+X0YXr0vELl" +
"1XrLTQIDAQABAkAZPvl/rwWWhfVNyAmmGC0jYrpyx5HVecFDmw1x6RZAk627x0ib" +
"7vnsbPpww7plkBz8KO7EEtkITTBb+hgaI5SBAiEA3KEwutED8Cz5GWRNTvDWgZyl" +
"YEKLVJ8pudv4rLrwmqECIQDWq5a+5akQ/ZTzojTyNx2neVeWtFKpvsLpmlgiBk99" +
"LQIhAJFfLCqL+hpQp7lRW5b+HXi9bEIm1oNldCrgg9PcQRjBAiAiMmNpNZyhIERC" +
"K2zTyQPoUeQqFb+1WrYiEHxJr0eqYQIhALHqunbd2sJRQ46OmPnFn35AMLAsfjvA" +
"yLUQXq+M3/O4";

        public void OpenNavigatorWithMultiRoute(Order order)
        {
            if (order == null)
            {
#if RELEASE
              return;
#else
                order = new Order();
                order.AddressFromLat = 55.719294;
                order.AddressFromLng = 37.593301;
                order.AddressToLat = 55.767324;
                order.AddressToLng = 37.591241;
#endif

            }
            
            var u = GetMultiRouteUri(order);
            if (u == null) return;

            Android.Net.Uri uri = u;
            Intent intent = new Intent(Intent.ActionView, uri);
            intent.SetPackage("ru.yandex.yandexnavi");

            // Проверяеем, установлена ли приложение навигатора
            PackageManager packageManager = MainActivity.Instance.PackageManager;
            IList<ResolveInfo> activities = packageManager.QueryIntentActivities(intent, 0);
   
            if (activities.Count > 0)
            {
                //Запускаем Яндекс.Навигатор
                MainActivity.Instance.StartActivity(intent);
            }
            else
            {
                // Открываем страницу Яндекс.Навигатора в Google Play.
                intent = new Intent(Intent.ActionView);
                intent.SetData(Android.Net.Uri.Parse("market://details?id=ru.yandex.yandexnavi"));
                MainActivity.Instance.StartActivity(intent);
            }
        }

        private Android.Net.Uri GetMultiRouteUri(Order order)
        {
            try
            {
                var curLoc = MainActivity.GetCurrentLocation();
                var targetLoc = new GeoLocation(order.AddressToLat, order.AddressToLng);
                GeoLocation nextLoc = null;
                if (order.Status <= OrderStatus.OnWayToPatient)
                {
                    nextLoc = new GeoLocation(order.AddressFromLat, order.AddressFromLng);
                }
                
                //если текущая локация неизвестна или целевой дислокации нет, выходим
                if ((curLoc?.Latitude ?? 0) == 0 || (curLoc?.Longitude ?? 0) == 0
                    || (targetLoc?.Latitude ?? 0) == 0 || (targetLoc?.Longitude ?? 0) == 0)
                    return null;
                var builder = Android.Net.Uri.Parse("yandexnavi://build_route_on_map")
                    .BuildUpon()
                    .AppendQueryParameter("lat_from", curLoc.Latitude.ToString().Replace(",", "."))
                    .AppendQueryParameter("lon_from", curLoc.Longitude.ToString().Replace(",", "."))
                    .AppendQueryParameter("lat_to", targetLoc.Latitude.ToString().Replace(",", "."))
                    .AppendQueryParameter("lon_to", targetLoc.Longitude.ToString().Replace(",", "."));
                   

                if (nextLoc != null)
                {
                    builder.AppendQueryParameter("lat_via_0", nextLoc.Latitude.ToString().Replace(",", "."));
                    builder.AppendQueryParameter("lon_via_0", nextLoc.Longitude.ToString().Replace(",", "."));
                }
                builder.AppendQueryParameter("client", "096");

                var tempUri = builder.Build();
                var uri = tempUri
                    .BuildUpon()
                    .AppendQueryParameter("signature", GetSha256(Key, tempUri.ToString()))
                    .Build();
            
                return uri;
            }
            catch
            {
                return null;
            }
        }

        private string GetSha256(string key, string data)
        {
            string trimmedKey = key.Replace("-----\\w+ PRIVATE KEY-----", "").Replace("\\s", "");

            try
            {
                byte[] result = Android.Util.Base64.Decode(trimmedKey, Android.Util.Base64Flags.Default);

                var str = new Java.Lang.String(data);
                var factory = Java.Security.KeyFactory.GetInstance("RSA");
                Java.Security.Spec.EncodedKeySpec keySpec = new Java.Security.Spec.PKCS8EncodedKeySpec(result);
                var signature = Java.Security.Signature.GetInstance("SHA256withRSA");
                var s = factory.GeneratePrivate(keySpec);
                signature.InitSign(s);
                signature.Update(str.GetBytes());

                byte[] encrypted = signature.Sign();
                return Android.Util.Base64.EncodeToString(encrypted, Android.Util.Base64Flags.NoWrap);
            }
            catch (Exception e)
            {
            }
            return null;
        }
    }
}
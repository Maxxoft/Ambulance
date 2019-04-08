using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.Platform.Android;
using Ambulance.Droid.ExtendedControls;
using Xamarin.Forms;
using Java.Interop;
using Plugin.Geolocator;
using System.Threading.Tasks;
using Android.Locations;
using Ambulance.ExtendedControls;
using Ambulance.ObjectModel;
using Android.Content;

[assembly: ExportRenderer(typeof(WebviewMap), typeof(WebviewMapRenderer))]
namespace Ambulance.Droid.ExtendedControls
{

	public class TestLoca
	{
		public string x;
		public string y;
	}


	public class WebviewMapRenderer : WebViewRenderer
	{
        public WebviewMapRenderer(Context t): base(t)
        {
            
        }

        private WebviewJSBridge WebviewJSBridg;

		protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.WebView> e)
		{
			base.OnElementChanged(e);


			try
			{

				if (Control != null)
				{
					var MapView = e.NewElement as WebviewMap;
					if (MapView != null)
					{
                        MapView.CurrentLoc = MainActivity.GetCurrentLocation();
						if (MapView.CurrentLoc == null && MapView.MapId != MapType.CarLocation)
						{
							MapView.ErrorOptimalRoute("");
							return;
						}

                        var CurrLocX = MapView.CurrentLoc.Latitude.ToString().Replace(',', '.');
                        var CurrLocY = MapView.CurrentLoc.Longitude.ToString().Replace(',', '.');

						WebviewJSBridg = new WebviewJSBridge(this);
						Control.Settings.JavaScriptEnabled = true;
						Control.AddJavascriptInterface(WebviewJSBridg, "CSharp");



						string Url = "";
						if (MapView.MapId == MapType.CarLocation)
						{
							Url = "Map.html?Longitude=" + 
								MapView.GeoCoords[0].Replace(",",".") + 
							    "&Latitude=" + MapView.GeoCoords[1].Replace(",", ".") + "&Maptype=CarLocation" +
								"&Array0=" + CurrLocX +
								"&Array1=" + CurrLocY;
						}
					
                        if (MapView.MapId == MapType.SimpleRoute)
                        {
                            //    string mapType = (MapView.MapId == BoosterSide.MapType.SimpleRoute) ? "ShowRoute" : "NewOrders";
                            string mapType = "NewOrders";
                            Url = "Map.html?Longitude=" + CurrLocX + "&Latitude=" + CurrLocY + "&Maptype=" + mapType;
                            Url += "&Array" + 0 + "=" + CurrLocX;
							Url += "&Array" + 1 + "=" + CurrLocY;
                            if (MapView.Order != null)
                            {
                                int Counter = 2;
                                Url += "&Array" + (Counter) + "=" + MapView.Order.AddressFromLat.ToString().Replace(",", ".");
                                Counter++;
								Url += "&Array" + (Counter) + "=" + MapView.Order.AddressFromLng.ToString().Replace(",", ".");
                                Counter++;
                                Url += "&PtsType0=" + GetPmType(MapView.Order.ArrivalDate);
                                Url += "&Array" + (Counter) + "=" + MapView.Order.AddressToLat.ToString().Replace(",", ".");
                                Counter++;
                                Url += "&Array" + (Counter) + "=" + MapView.Order.AddressToLng.ToString().Replace(",", ".");
                                Counter++;
                            }
                        }

                        if (MapView.MapId == MapType.OptimalRoute)
                        {
                            //    string mapType = (MapView.MapId == BoosterSide.MapType.SimpleRoute) ? "ShowRoute" : "NewOrders";
                            string mapType = "NewOrders";
                            Url = "Map.html?Longitude=" + CurrLocX + "&Latitude=" + CurrLocY + "&Maptype=OptimalRoute";
                            Url += "&Array" + 0 + "=" + CurrLocX;
                            Url += "&Array" + 1 + "=" + CurrLocY;
                            if (MapView.Order != null)
                            {
                                int Counter = 2;
                                Url += "&Array" + (Counter) + "=" + MapView.Order.AddressFromLat.ToString().Replace(",", ".");
                                Counter++;
                                Url += "&Array" + (Counter) + "=" + MapView.Order.AddressFromLng.ToString().Replace(",", ".");
                                Counter++;
                                    /*Url += "&PtsType" + i + "=" + GetPmType(MapView.Orders[i].ArrivalDate);
                                    Url += "&Array" + (Counter) + "=" + MapView.Orders[i].AddressToLat.ToString().Replace(",", ".");
                                    Counter++;
                                    Url += "&Array" + (Counter) + "=" + MapView.Orders[i].AddressToLng.ToString().Replace(",", ".");
                                    Counter++;*/
                            }
                        }

                        /*if (MapView.MapId == MapType.OptimalRoute)
                        {
                            Url = "Map.html?Longitude=" + CurrLocX + "&Latitude=" + CurrLocY + "&Maptype=OptimalRoute";

                            Url += "&Array" + 0 + "=" + CurrLocX;
							Url += "&Array" + 1 + "=" + CurrLocY;
                            if (MapView.Orders != null)
                            {
                                WebviewJSBridg.Locations = new List<Order>(MapView.Orders);
                                WebviewJSBridg.Locations.Add(new Order() 
                                { 
                                    Latitude = Convert.ToDouble(CurrLocX, System.Globalization.CultureInfo.InvariantCulture), 
                                    Longitude = Convert.ToDouble(CurrLocY, System.Globalization.CultureInfo.InvariantCulture) 
                                }); 
                                WebviewJSBridg.RouteCounts = WebviewJSBridg.Locations.Count * WebviewJSBridg.Locations.Count;


                                int Counter = 2;
                                for (int i = 0; i < MapView.Orders.Count; i++)
                                {

                                    Url += "&Array" + (Counter) + "=" + MapView.BoosterOrders[i].Latitude.ToString().Replace(",", ".");
                                    Counter++;
                                    Url += "&Array" + (Counter) + "=" + MapView.BoosterOrders[i].Longitude.ToString().Replace(",", ".");
                                    Counter++;
                                
                              }
                            }

                        }
                        if (MapView.MapId == BoosterSide.MapType.NewOrders)
                        {
                            int Counter = 0;
                            for (int i = 0; i < MapView.NewOrders.Count; i++)
                            {
								Url += "&Points" + (Counter) + "=" + MapView.NewOrders[i].Latitude.ToString().Replace(",", ".");
                                Counter++;
								Url += "&Points" + (Counter) + "=" + MapView.NewOrders[i].Longitude.ToString().Replace(",", ".");
                                Counter++;
                                Url += "&PmType" + i + "=" + GetPmType(MapView.NewOrders[i].ToDate);
                            }
                        }

                      

						if (MapView.MapId == MapType.SimpleRoute)
						{
							int Counter = 0, Counter2 = 0;
							double Distance = 0;
							double MaxRadius = 2;
                            bool NewOrderInRaduis = false;

							for (int i = 0; i < MapView.Orders.Count; i++)
							{
                                Distance = TrackingHelper.CalculateDistance(
                                 new TrackingHelper.Location()
                                 {
									Latitude = MapView.Orders[i].AddressFromLat,
									Longitude = MapView.Orders[i].AddressFromLng
                                 },
                                new TrackingHelper.Location()
                                {
                                    Latitude = Convert.ToDouble(CurrLocX, System.Globalization.CultureInfo.InvariantCulture),
                                    Longitude = Convert.ToDouble(CurrLocY, System.Globalization.CultureInfo.InvariantCulture)
                                });

                                if (Distance < MaxRadius) NewOrderInRaduis = true;
                                else
                                {
                                    for (int j = 0; j < MapView.Orders.Count; j++)
                                    {
                                        Distance = TrackingHelper.CalculateDistance(
                                              new TrackingHelper.Location()
                                             {
											Latitude = MapView.NewOrders[i].Latitude,
                                            Longitude = MapView.NewOrders[i].Longitude
                                             },
                                               new TrackingHelper.Location()
                                             {
											Latitude = MapView.BoosterOrders[j].Latitude,
											Longitude = MapView.BoosterOrders[j].Longitude
                                             });

                                        if (Distance < MaxRadius)
                                        {
                                            NewOrderInRaduis = true;
                                            break;
                                        }
                                    }
                                }
                                if (!NewOrderInRaduis) continue;


                                Url += "&Points" + (Counter) + "=" + MapView.NewOrders[i].Latitude.ToString().Replace(",", ".");
								Counter++;
                                Url += "&Points" + (Counter) + "=" + MapView.NewOrders[i].Longitude.ToString().Replace(",", ".");
								Counter++;
								Url += "&PmType" + Counter2 + "=" + GetPmType(MapView.NewOrders[i].ToDate); 
								Counter2++;
							}
						}*/

                        e.NewElement.Source = new UrlWebViewSource() { Url = System.IO.Path.Combine("file:///android_asset/", Url) };
                    }
                }

            }
            catch (System.Exception ex)
            {
            }
        }

        private void GetTempOrder()
        {

        }


        private int GetPmType(DateTime? ToDate) // 0 - red, 1 - orange, 2 = green
        {
            if (ToDate == null) return 0;


            var ExpiredTime = TimeSpan.FromTicks(ToDate.Value.Ticks - DateTime.Now.Ticks).TotalHours;

            if (ExpiredTime < 2)
                return 0;
            if (ExpiredTime < 4)
                return 1;

            return 2;
        }
    }

    public static class TrackingHelper
    {
        public class Location
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        public static double CalculateDistance(Location location1, Location location2)
        {
            double circumference = 40000.0;
            double distance = 0.0;

            //Calculate radians
            double latitude1Rad = DegreesToRadians(location1.Latitude);
            double longitude1Rad = DegreesToRadians(location1.Longitude);
            double latititude2Rad = DegreesToRadians(location2.Latitude);
            double longitude2Rad = DegreesToRadians(location2.Longitude);

            double logitudeDiff = Math.Abs(longitude1Rad - longitude2Rad);

            if (logitudeDiff > Math.PI)
            {
                logitudeDiff = 2.0 * Math.PI - logitudeDiff;
            }

            double angleCalculation =
                Math.Acos(
                  Math.Sin(latititude2Rad) * Math.Sin(latitude1Rad) +
                  Math.Cos(latititude2Rad) * Math.Cos(latitude1Rad) * Math.Cos(logitudeDiff));

            distance = circumference * angleCalculation / (2.0 * Math.PI);

            return distance;
        }
    }

    public class WebviewJSBridge : Java.Lang.Object
    {
        private WebviewMapRenderer WebviewMap;
        private WebviewMap WebView;
        public List<TspImplementaion.Location> TableDistance = new List<TspImplementaion.Location>();
        public List<Order> Locations = new List<Order>();
        public int RouteCounts = 0;
        public int _RouteCounts = 0;
        public bool HasError = false;
        public int temp = 0;
        public WebviewJSBridge(WebviewMapRenderer WebviewMap)
        {
            this.WebviewMap = WebviewMap;
            WebView = WebviewMap.Element as WebviewMap;
        }

        [Export]
        [Android.Webkit.JavascriptInterface]
        public async void GetAllRoutes(Java.Lang.String Distance, Java.Lang.String Loc1x, Java.Lang.String Loc1y, Java.Lang.String Loc2x, Java.Lang.String Loc2y, Java.Lang.String Time)
        {
            double.TryParse(Distance.ToString(), out double dist);
            double.TryParse(Loc1x.ToString(), out double loc1x);
            double.TryParse(Loc1y.ToString(), out double loc1y);
            double.TryParse(Loc2x.ToString(), out double loc2x);
            double.TryParse(Loc2y.ToString(), out double loc2y);

            if /*(Math.Abs(WebView.Order.AddressFromLat - loc1x) <= 0.01 && Math.Abs(WebView.Order.AddressFromLng - loc1y) <= 0.01 && Math.Abs(WebView.CurrentLoc.Latitude - loc2x) <= 0.01 && Math.Abs(WebView.CurrentLoc.Longitude - loc2y) <= 0.01) ||*/
                (Math.Abs(WebView.CurrentLoc.Latitude - loc1x) <= 0.01 && Math.Abs(WebView.CurrentLoc.Longitude - loc1y) <= 0.01 && Math.Abs(WebView.Order.AddressFromLat - loc2x) <= 0.01 && Math.Abs(WebView.Order.AddressFromLng - loc2y) <= 0.01)
            {
                if (dist > 0) WebView.Order.Distance = Math.Round(dist / 1000, 2);
                WebView.Order.CalcDistanceDate = DateTime.Now;
                WebView.CurrentLoc.Latitude = 0;
                WebView.CurrentLoc.Longitude = 0;
                WebView.ParentPage?.RouteCalculated();
            }

            /*try
            {
                Dist = Convert.ToDouble(Distance.ToString().Replace(".", ","), System.Globalization.CultureInfo.InvariantCulture);
                var time = Convert.ToDouble(Time.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                int Rowid = Locations.FindIndex(x => x.AddressFromLat.ToString().TrimEnd(new Char[] { '0' }).Replace(",", ".") == Loc1x.ToString().Replace(",", ".") 
                                                && x.AddressFromLng.ToString().TrimEnd(new Char[] { '0' }).Replace(",", ".") == Loc1y.ToString().Replace(",", "."));
                int Columnid = Locations.FindIndex(x => x.AddressFromLat.ToString().TrimEnd(new Char[] { '0' }).Replace(",",".") == Loc2x.ToString().Replace(",", ".") 
                                                   && x.AddressFromLng.ToString().TrimEnd(new Char[] { '0' }).Replace(",", ".") == Loc2y.ToString().Replace(",", "."));

                if (TableDistance.FindIndex(x => x.Id == Rowid) == -1)
                    TableDistance.Add(new TspImplementaion.Location()
                    {
                        Id = Rowid,
                        Distance = new Dictionary<int, double>(),
                        //OrderId = Locations[Rowid].ServiceId,
                        Time = new Dictionary<int, double>()
                    });

                TableDistance[TableDistance.FindIndex(x => x.Id == Rowid)].Distance.Add(Columnid, Dist);
                TableDistance[TableDistance.FindIndex(x => x.Id == Rowid)].Time.Add(Columnid, time);
      
                _RouteCounts++;

                if (RouteCounts == _RouteCounts)
                    SetOptimalRoute();
            }
            catch (Java.Lang.Exception ex)
            {
            }
            catch (System.Exception ex)
            {
                if (!HasError)
                {
                    if (TableDistance.ToList().FindIndex(x => x.Distance.ContainsKey(-1)) != -1)
                    {
                        HasError = true;
                        WebView.ErrorOptimalRoute();
                    }
                }
            }*/
        }

        private void SetOptimalRoute()
        {
            /*if (TableDistance.ToList().FindIndex(x => x.Distance.ContainsKey(-1)) != -1)
            {
                if (!HasError)
                    WebView.ErrorOptimalRoute();
                return;
            }

            var startLoc = TableDistance.Where(x => x.OrderId == -50).FirstOrDefault();
            TableDistance.Remove(TableDistance.FirstOrDefault(x => x.OrderId == -50));

            var algorithm = new TspImplementaion.TSPAlgorithm(startLoc, TableDistance.ToArray(), 100);

            IEnumerable<TspImplementaion.Location> bestSolutionSoFar = algorithm.GetBestSolutionSoFar().ToArray();
            for (int i = 0; i < 100; i++)
            {
                algorithm.Reproduce();
                bestSolutionSoFar = algorithm.GetBestSolutionSoFar().ToArray();
            }
            var resultedList = bestSolutionSoFar.ToList();
            resultedList.Insert(0, startLoc);
            double totalTime = 0;
            double totalLenght = 0;
            for (int i = 0; i < resultedList.Count -1; i++)
            {
                totalLenght += resultedList[i].Distance[resultedList[i + 1].Id];
                totalTime += resultedList[i].Time[resultedList[i + 1].Id];
            }

            var res = (long)TspImplementaion.Location.GetTotalDistance(startLoc, (TspImplementaion.Location[])bestSolutionSoFar);

            */
            /*var result = Locations.OrderBy(x => resultedList.FindIndex(y => y.OrderId == x.ServiceId)).ToList();
            for (int i = 1; i < result.Count; i++)
            {
                result[i].TimeToFromPrev = TimeSpan.FromSeconds(resultedList[i-1].Time[resultedList[i].Id]);
                if (i > 1)
                    result[i].TimeFromStartPoint = TimeSpan.FromSeconds(resultedList[i-1].Time[resultedList[i].Id]) + result[i - 1].TimeFromStartPoint;
                else result[i].TimeFromStartPoint = TimeSpan.FromSeconds(resultedList[i - 1].Time[resultedList[i].Id]);

            }
            WebView.SetOrdersOrder(result, totalLenght, totalTime);*/
            
        }

        [Export]
        [Android.Webkit.JavascriptInterface]
        public void SetAddress(Java.Lang.String Address)
        {
            string S = null;
            S = Address.ToString();
            temp++;
        }
        [Export]
        [Android.Webkit.JavascriptInterface]
        public void SetCoords(Java.Lang.String Latitude, Java.Lang.String Longitude)
        {
            try
            {
                var TempArray = new string[2];
                if (Latitude == null || Latitude.ToString() == "" || Longitude == null || Longitude.ToString() == "")
                    return;
                TempArray[0] = Latitude.ToString();
                TempArray[1] = Longitude.ToString();
                WebView.GeoCoords = TempArray;
            }
            catch (System.Exception ex)
            { }
        }
        [Export]
        [Android.Webkit.JavascriptInterface]
        public void MapLoaded()
        {
            WebView.MapWasLoaded = true;
            Dispose(true);
        }
        [Export]
        [Android.Webkit.JavascriptInterface]
        public void OpenPage(Java.Lang.String PageType, Java.Lang.String MapX, Java.Lang.String MapY)
        {
            if (PageType.ToString() == "neworders")
				WebView.OpenPage(Pages.PageType.FreeOrders, MapX.ToString(), MapY.ToString());
            if (PageType.ToString() == "btsrorder")
                WebView.OpenPage(Pages.PageType.ActiveOrder, MapX.ToString(), MapY.ToString());
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            { WebviewMap = null; GC.Collect(); }
        }
    }
}
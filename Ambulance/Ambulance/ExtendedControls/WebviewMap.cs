using Ambulance.ObjectModel;
using Ambulance.Pages;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Ambulance.ExtendedControls
{
    public enum MapType
    {
        CarLocation,
        SimpleRoute,
        NewOrders,
        OptimalRoute
    }

    public interface IMapPage
    {
        void MapLoaded();
        void RouteCalculated();
        void OpenPage(PageType Type, string MapX, string Map);
        void MapErrorAsync(string Error);
    }

    public class WebviewMap : WebView
    {
        public IMapPage ParentPage;
        private bool? _MapWasLoaded;

        public MapType? MapId { get; set; } = null;
        public string[] GeoCoords { get; set; }

        public bool? MapWasLoaded
        {
            get { return _MapWasLoaded; }
            set
            {
                _MapWasLoaded = value;
                if (value == true)
                      ParentPage.MapLoaded();
            }
        }

        public List<Order> Orders { get; set; }

        public void Clear()
        {
            GeoCoords = null;
            ParentPage = null;
            this.Source = null;
        }

        public void SetOrdersOrder(List<Order> SortedOrders, double TotalLenght, double TotalTime)
        {
            /*SortedOrders.Remove(SortedOrders.Find(x=> x.ServiceId == -50));
            for (int i = 0; i < SortedOrders.Count; i++)
                SortedOrders[i].ExecuteId = i + 1;*/

            //BoosterOrders = SortedOrders;
            ParentPage.RouteCalculated();
        }

        public void OpenPage(PageType Type, string MapX, string MapY)
        {
            ParentPage.OpenPage(Type, MapX, MapY);
        }

        public WebviewMap()
        {
            MapWasLoaded = null;
        }

        public void ErrorOptimalRoute(string Error = null)
        {
            ParentPage.MapErrorAsync(Error);
        }
    }

}

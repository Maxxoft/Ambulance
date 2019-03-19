using Ambulance.ExtendedControls;
using Ambulance.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Ambulance.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MapPage : ContentPage, IMapPage
	{
		public MapPage ()
		{
			InitializeComponent ();
		}

        public MapPage(Order order, MapType mapType)
        {
            InitializeComponent();
            wwMap.MapId = mapType;
            if (mapType == MapType.SimpleRoute)
            {
                Title = "Маршрут выполнения заказа";
                wwMap.Order = order;
            }
            else if (mapType == MapType.NewOrders)
            {
                Title = "Карта заказов";
                wwMap.MapId = mapType;
            }
            SetContent();
        }

        public MapPage(double latitude, double longitude)
        {
            InitializeComponent();
            Title = "Местоположение пациента";
            wwMap.MapId = MapType.CarLocation;
            wwMap.GeoCoords = new string[2] { latitude.ToString(), longitude.ToString() };
            SetContent();
        }

        private void SetContent()
        {
            ErrorLB.IsVisible = false;
            wwMap.ParentPage = this;
            Indicator.IsVisible = true;
            Indicator.IsRunning = true;
            Indicator.Color = Color.Orange;
            Indicator.HeightRequest = 100;
        }

        public async void MapErrorAsync(string Error)
        {
            try
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Indicator.IsVisible = false;
                    Indicator.IsRunning = false;
                    ErrorLB.Text = Error;
                    ErrorLB.IsVisible = true;
                });
                await Task.Delay(2000);
                MainPage.Instance?.SwitchToPage(PageType.FreeOrders, false);
                //Navigation.PushAsync(new BoosterMainPage(PageType.FreeOrders));

            }
            catch { }
        }

        public void MapLoaded()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Indicator.IsVisible = false;
                Indicator.IsRunning = false;
            });
        }

        public void OpenPage(PageType Type, string MapX, string Map)
        {
            throw new NotImplementedException();
        }

        public void RouteCalculated()
        {
            throw new NotImplementedException();
        }
    }
}
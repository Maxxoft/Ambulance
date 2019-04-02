using System;
using System.Linq;
using Xamarin.Forms;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Services;
using Ambulance.Pages.Popup;
using Ambulance.Data;
using Ambulance.WebService;
using Ambulance.Dependency;
using Ambulance.ObjectModel;
using System.Collections.Generic;

namespace Ambulance.Pages
{
    public class MainPage : MasterDetailPage
    {
        MasterPage masterPage;
		OrdersPage curPage;
        //readonly AppBusyPage busyPopup = new AppBusyPage();

        public static MainPage Instance { get; private set; }

        public MainPage(PageType pageType)
        {
            MasterBehavior = MasterBehavior.Popover;
            masterPage = new MasterPage();
            masterPage.ListView.ItemSelected += OnItemSelected;
            Master = masterPage;

            //curPage = new OrdersPage(AppData.Crew?.ActiveOrder?.OrderId > 0 ? PageType.ActiveOrder : PageType.FreeOrders);
            curPage = new OrdersPage(PageType.ActiveOrder);
            Detail = new NavigationPage(curPage) { BarBackgroundColor = Color.Orange };
			Instance = this;

            StartRefreshTimer();
        }


        bool refreshTimerActive;
        //int newOrdersAmount;
        Order oldActiveOrder;
        void StartRefreshTimer()
        {
            DependencyService.Get<IAPIHelper>().RequestLocationsPermissions();

            if (refreshTimerActive) return;
            refreshTimerActive = true;
            Task.Run(async () =>
            {
                while (refreshTimerActive)
                {
                    await Task.Delay(60000);

                    while (curPage?.IsBusy == true)
                        await Task.Delay(1000);

                    if (!refreshTimerActive) return;
                    var currentCoord = DependencyService.Get<IAPIHelper>().GetCurrentLocation();
                    if (currentCoord != null && currentCoord.Latitude > 1 && currentCoord.Longitude > 1)
                        await ApiService.UpdateLocation(currentCoord.Latitude, currentCoord.Longitude);

                    while (curPage?.IsBusy == true)
                        await Task.Delay(1000);
                    AppData.StoreDistances();

                    //if (!refreshTimerActive) return;
                    //await ApiService.GetNewOrders();

                    while (curPage?.IsBusy == true)
                        await Task.Delay(1000);

                    oldActiveOrder = null;
                    if (AppData.Crew.ActiveOrder != null)
                        oldActiveOrder = new Order
                        {
                            OrderId = AppData.Crew.ActiveOrder.OrderId,
                            Status = AppData.Crew.ActiveOrder.Status
                        };
                    if (!refreshTimerActive) return;
                    await ApiService.GetActiveOrder();

                    //newOrdersAmount += AppData.RestoreDistances();
                    AppData.RestoreDistances();

                    while (curPage?.IsBusy == true)
                        await Task.Delay(1000);

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (!(curPage is OrdersPage page)) return;
                        page.NotifyActiveOrder(oldActiveOrder);
                    });

                    /*Device.BeginInvokeOnMainThread(() =>
                    {
                        if (!(curPage is OrdersPage page)) return;
                        if (page.PageType == PageType.ActiveOrder)
                        {
                            if ((AppData.Crew.ActiveOrder?.Status ?? OrderStatus.Cancelled) == OrderStatus.Cancelled)
                            {
                                curPage.PageType = PageType.FreeOrders;
                            }
                            else if (newOrdersAmount > 0)
                                page.SetAlertNewOrders(newOrdersAmount);
                        }
                        else if (page.PageType == PageType.FreeOrders)
                        {
                            if ((AppData.Crew.ActiveOrder?.Status ?? OrderStatus.Cancelled) < OrderStatus.Cancelled)
                            {
                                curPage.PageType = PageType.ActiveOrder;
                            }
                            else if (newOrdersAmount > 0)
                            {
                                DependencyService.Get<IAPIHelper>().PlayAlertSound();
                                page.RefreshContent();
                                newOrdersAmount = 0;
                            }
                        }
                    });*/
                }
            });
        }

        void StopRefreshTimer()
        {
            refreshTimerActive = false;            
        }

		void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (!(e.SelectedItem is MasterPageItem item)) return;
            if (item.Pagetype == PageType.LogOut)
            {
                refreshTimerActive = false; 
                Settings.AppState.LastPhone = 0;
                Settings.AppState.LastPin = null;
                Settings.AppState.LastLoginDate = DateTime.MinValue;
                Settings.AppState.Save();
                App.Instance.MainPage = new AuthorizationPage();
                return;
            }
			SwitchToItem(item);
        }

        void SwitchToItem(MasterPageItem item)
        {
            if (item == null) return;
            if (item.TargetType != typeof(OrdersPage)) return;

            if (curPage == null)
            {
                curPage = new OrdersPage(item.Pagetype);
                Detail = new NavigationPage(curPage) { BarBackgroundColor = Color.Orange };
            }
            else
                curPage.PageType = item.Pagetype;
            masterPage.ListView.SelectedItem = null;
            IsPresented = false;
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(50);
                if (PopupNavigation.Instance.PopupStack.Count > 0)
                    await PopupNavigation.Instance.PopAllAsync();
                /*if (Detail.Navigation.NavigationStack.Count > 0)
                    await Detail.Navigation.PopAllPopupAsync();*/
            });
		}
    }
}
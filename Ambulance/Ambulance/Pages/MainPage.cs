﻿using System;
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
		ContentPage curPage;
        readonly AppBusyPage busyPopup = new AppBusyPage();

        public static MainPage Instance { get; private set; }

        public MainPage(PageType pageType)
        {
            MasterBehavior = MasterBehavior.Popover;
            masterPage = new MasterPage();
            masterPage.ListView.ItemSelected += OnItemSelected;
            Master = masterPage;

            //curPage = new BoosterMainPage(PageType.BoosterOrders, newOrdersAmount);
            //Detail = new NavigationPage(curPage) { BarBackgroundColor = Color.Orange };
			Instance = this;

            SwitchToPage(pageType, false);

            StartRefreshTimer();
        }


        bool refreshTimerActive;
        int newOrdersAmount;
        void StartRefreshTimer()
        {
            if (refreshTimerActive) return;
            refreshTimerActive = true;
            Task.Run(async () =>
            {
                while (refreshTimerActive)
                {
                    await Task.Delay(60000);
                    AppData.StoreDistances();

                    // Refresh free orders
                    if (!refreshTimerActive) return;
                    await Task.Run(() => ApiService.GetNewOrders());
                    if (!refreshTimerActive) return;
                    if (AppData.Crew.ActiveOrder?.OrderId > 0)
                        AppData.Crew.ActiveOrder = await ApiService.GetOrderDetails(AppData.Crew.ActiveOrder.OrderId);

                    newOrdersAmount += AppData.RestoreDistances();
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (!(curPage is OrdersPage page)) return;
                        if (page.PageType == PageType.ActiveOrder && (AppData.Crew.ActiveOrder?.Status ?? OrderStatus.Cancelled) == OrderStatus.Cancelled)
                        {
                            page.CalcDistance = false;
                            SwitchToPage(PageType.FreeOrders, false);
                        }
                        else if (page.PageType == PageType.ActiveOrder && newOrdersAmount > 0)
                            page.SetAlertNewOrders(newOrdersAmount);
                        else if (page?.PageType == PageType.FreeOrders)
                        {
                            if (newOrdersAmount > 0)
                                DependencyService.Get<IAPIHelper>().PlayAlertSound();
                            page.RefreshContent();
                            newOrdersAmount = 0;
                        }
                    });
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
			SwitchToItem(item, true);
        }

        async void SwitchToItem(MasterPageItem item, bool refresh)
        {
            if (item == null) return;

            //if (item.TargetType != typeof(BoosterMainPage)) return;

            curPage = null;
            string res = null;

            if (refresh)
            {
                await Detail.Navigation.PushPopupAsync(busyPopup, true);
                AppData.StoreDistances();
                switch (item.Pagetype)
                {
                    case PageType.FreeOrders:
                        App.ClearTestLog();
                        res = await Task.Run(() => ApiService.GetNewOrders());
                        await App.ShowTestLog(this);
                        break;
                    case PageType.ActiveOrder:
                        App.ClearTestLog();
                        AppData.Crew.ActiveOrder = await Task.Run(() => ApiService.GetOrderDetails(AppData.Crew.ActiveOrder.OrderId));
                        await App.ShowTestLog(this);
                        break;
                }
                AppData.RestoreDistances();
            }

            curPage = new OrdersPage(item.Pagetype);
            Detail = new NavigationPage(curPage) { BarBackgroundColor = Color.Orange };
            masterPage.ListView.SelectedItem = null;
            IsPresented = false;
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(50);
                if (PopupNavigation.Instance.PopupStack.Count > 0)
                    PopupNavigation.Instance.PopAllAsync();
                /*if (Detail.Navigation.NavigationStack.Count > 0)
                    Detail.Navigation.PopAllPopupAsync();*/
            });
		}

		public void SwitchToPage(PageType pageType, bool refresh, Order selectedOrder = null)
		{
			SwitchToItem(new MasterPageItem
			{
				TargetType = typeof(OrdersPage),
				Title = OrdersPage.TitleByPageType(pageType),
                Pagetype = pageType
            }, refresh);
		}
    }
}
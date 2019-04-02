using Ambulance.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ambulance.Data
{
    public static class AppData
    {
        public static List<CrewType> CrewTypes { get; set; }
        public static Crew Crew { get; set; }
        public static List<Order> Orders { get; set; }

        public static void Save()
        {
            Settings.AppState.ActiveCrew = Crew;
            Settings.AppState.Save();
        }

        static List<Order> oldOrders;
        static Order curOrder;
        public static void StoreDistances()
        {
            oldOrders = new List<Order>();
            if (Orders != null)
            {
                foreach (var order in Orders)
                    oldOrders.Add(new Order { OrderId = order.OrderId, Distance = order.Distance, CalcDistanceDate = order.CalcDistanceDate });
            }
            curOrder = Crew?.ActiveOrder;
        }

        public static int RestoreDistances()
        {
            var res = 0;
            if (oldOrders != null)
            {
                foreach (var order in oldOrders)
                {
                    var newOrder = Orders?.FirstOrDefault(x => x.OrderId == order.OrderId);
                    if (newOrder != null)
                    {
                        newOrder.Distance = order.Distance;
                        newOrder.CalcDistanceDate = order.CalcDistanceDate;
                    }
                    else res++;
                }
            }

            if (curOrder != null && Crew?.ActiveOrder != null && curOrder.OrderId == Crew.ActiveOrder.OrderId)
            {
                Crew.ActiveOrder.Distance = curOrder.Distance;
                Crew.ActiveOrder.CalcDistanceDate = curOrder.CalcDistanceDate;
            }

            return res;
        }
    }
}

using Ambulance.ObjectModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ambulance.Data
{
    public static class AppData
    {
        public static Crew Crew { get; set; }
        public static List<Order> Orders { get; set; }

        public static void Save()
        {
            Settings.AppState.ActiveCrew = Crew;
            Settings.AppState.Save();
        }
    }
}

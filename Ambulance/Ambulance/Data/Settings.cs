using Ambulance.ObjectModel;
using Newtonsoft.Json;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System;

namespace Ambulance.Data
{
    public static class Settings
    {
        public static ISettings AppSettings
        {
            get { if (CrossSettings.IsSupported) return CrossSettings.Current; return null; }
        }

        #region Setting Constants

        //const string AppStateKey = "AppState";

        #endregion

        public static readonly AppState AppState = new AppState();
    }

    public class AppState
    {
        public AppState()
        {
            Load();
        }

        public void Load()
        {
            var settings = Settings.AppSettings;
            if (settings == null) return;
            var data = settings.GetValueOrDefault("AppState", default(string));
            try
            {
                if (data == null) return;
                JsonConvert.PopulateObject(data, this);
            }
            catch (Exception ex)
            {
                //GoogleAnalyticsHelper.SendException("AppState:Load " + ex.Message, false);
            }
        }

        public void Save()
        {
            var settings = Settings.AppSettings;
            if (settings == null) return;
            try
            {
                var json = JsonConvert.SerializeObject(this);
                settings.AddOrUpdateValue("AppState", json);
            }
            catch (Exception ex)
            {
                //GoogleAnalyticsHelper.SendException("AppState:Save " + ex.Message, false);
            }
        }

        public DateTime LastActivityDate { get; set; }
        public DateTime LastLoginDate { get; set; }
        public long LastPhone { get; set; }
        public string LastPin { get; set; }

        public Crew ActiveCrew { get; set; }
        public GeoLocation ActiveLocation { get; set; }

        public bool PushRegistered { get; set; }
        public bool PushRequested { get; set; }
        public DateTime CalcRouteTime { get; set; }
    }
}
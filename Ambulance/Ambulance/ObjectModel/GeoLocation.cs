using System;

namespace Ambulance.ObjectModel
{
	public class GeoLocation
	{
		public double Latitude { get; set; }
		public double Longitude { get; set; }
        public string Error { get; set; }
        public string Provider { get; set; }
        public int RequestAmount { get; set; }
        public DateTime StartRequestDate;
        public DateTime FirstResponseDate, LastResponseDate;
        public string Address { get; set; }

        public GeoLocation()
        {
            
        }

        public GeoLocation(double lat, double lon)
        {
            Latitude = lat;
            Longitude = lon;
        }
	}
}

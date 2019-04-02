using Ambulance.Helper;
using System;

namespace Ambulance.ObjectModel
{
    public class Crew 
    {
		public long? ServiceID { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }
        public GeoLocation Location { get; set; }
        public DateTime DateArrival { get; set; }
        public DateTime DateLastLocationUpdate { get; set; }

        public Order ActiveOrder { get; set; }
        public bool Online { get; set; }
        public int CrewTypeId { get; set; }
    }

    public class CrewType
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}

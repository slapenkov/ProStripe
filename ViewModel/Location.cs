using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProStripe.ViewModel
{
    public class Location
    {
        public double latitude { get; set; }
        public double longitude { get; set; }

        private const double radians = Math.PI / 180;   // radians per degree
        private const double R = 6371009;               // Earth average radius in meters

        public Location() 
        { latitude = longitude = 0; }

        public Location(double lat, double lon)
        { latitude = lat; longitude = lon; }

        // Distance between locations
        // Haversine formula from http://www.movable-type.co.uk/scripts/latlong.html

        public double distance(Location to)
        {
            Location d;
            d = new Location(this.latitude - to.latitude, 
                             this.longitude - to.longitude);
            double a, c, distance;
            a = Math.Pow(Math.Sin(d.latitude / 2 * radians), 2) +
                Math.Pow(Math.Sin(d.longitude / 2 * radians), 2) * 
                Math.Cos(this.latitude * radians) * Math.Cos(to.latitude * radians);
            c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            distance = R * c;  // angle to meters
            return distance;
        }

        // Initial great circle bearing to another location
        // Ibid

        public double bearing(Location to)
        {
            double x, y, dLon, course;
            dLon = this.longitude - to.longitude;
            y = Math.Sin(dLon * radians) * Math.Cos(to.latitude * radians);
            x = Math.Cos(this.latitude * radians) * Math.Sin(to.latitude * radians) -
                Math.Sin(this.latitude * radians) * Math.Cos(to.latitude * radians) * Math.Cos(dLon * radians);
            course = Math.Atan2(y, x) / radians;
            course = (course + 360) % 360;  // degrees 0..360
            return course;
        }
    }
}

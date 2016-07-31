using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProStripe.ViewModel
{
    public class RaceRenderPoint
    {
        public DateTime timestamp { get; private set; }
        public DateTime date { get; private set; }
        public double latitude { get; private set; }
        public double longitude { get; private set; }
        public double knots { get; private set; }
        public double course { get; private set; }
        public string NS { get; private set; }
        public string EW { get; private set; }
        public double x { get; private set; }
        public double y { get; private set; }
        public double z { get; private set; }

        public double xFiltered { get; set; }
        public double yFiltered { get; set; }
        public double zFiltered { get; set; }


        public RaceRenderPoint() { }

        public RaceRenderPoint(DateTime timestamp, DateTime date, double latitude, double longitude, double knots, double course, string NS, string EW, double x, double y, double z)
        {
            this.timestamp = timestamp;
            this.date = date;
            this.latitude = latitude;
            this.longitude = longitude;
            this.knots = knots;
            this.course = course;
            this.NS = NS;
            this.EW = EW;
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProStripe.ViewModel
{
    public class DataProcessor
    {
        private List<RaceRenderPoint> raceRenderData;

        private Location where { get; set; }
        private DateTime when { get; set; }

        public const string csvHead = "Time,Latitude,Longitude,Altitude,X,Y,Z,XS,YS,ZS,mph,bearing,Knots,kts,meters,Date";



        public DataProcessor()
        {
            this.raceRenderData = new List<RaceRenderPoint>();
        }



        public void AddData(NMEA g)
        {
            RaceRenderPoint point = new RaceRenderPoint(g.timestamp, g.date, g.latitude, g.longitude, g.knots, g.course, g.NS, g.EW, g.x, g.y, g.z);
            raceRenderData.Add(point);
        }


        public void Smooth()
        {
            double[] xNoisy = new double[this.raceRenderData.Count];
            double[] yNoisy = new double[this.raceRenderData.Count];
            double[] zNoisy = new double[this.raceRenderData.Count];

            //extract accelerator data from collection
            int c = 0;
            foreach (RaceRenderPoint d in raceRenderData)
            {
                xNoisy[c] = d.x;
                yNoisy[c] = d.y;
                zNoisy[c] = d.z;
                c++;
            }

            //smoothing
            Filter xFilter = new Filter(xNoisy, 3, 0.8);
            Filter yFilter = new Filter(yNoisy, 3, 0.8);
            Filter zFilter = new Filter(zNoisy, 3, 0.8);

            //store smoothed data to collection
            c = 0;
            foreach(RaceRenderPoint d in raceRenderData)
            {
                d.xFiltered = xFilter.cleanData[c];
                d.yFiltered = yFilter.cleanData[c];
                d.zFiltered = zFilter.cleanData[c];
                c++;
            }
        }

        public void WriteData(Subtitles subtitles)
        {
            //store data to subtitle file
            foreach (var d in raceRenderData)
            {
                subtitles.WriteLine(this.formatCSV(d));
            }
        }

        private string formatCSV(RaceRenderPoint point)
        {
            //same as NMEA, but modified for RaceRenderPoint object
            double minutes = point.latitude % 100;
            double lat = (point.latitude - minutes) / 100 + minutes / 60;
            lat = (point.NS == "S" ? -lat : lat);
            minutes = point.longitude % 100;
            double lon = (point.longitude - minutes) / 100 + minutes / 60;
            lon = (point.EW == "W" ? -lon : lon);
            double mph = point.knots;
            double meters = 0;
            double bearing = 0;
            double kts = 0;
            double feet = point.course; // course * 3.28084 to convert meters to feet 

            Location here = new Location(lat, lon);
            if (where != null)
            {
                meters = where.distance(here);
                bearing = where.bearing(here);
                TimeSpan elapsed = point.timestamp.Subtract(when);
                double ms = elapsed.TotalMilliseconds;
                if (ms > 0)
                    kts = meters * 1000 / ms / 1852;
            }
            where = here;
            when = point.timestamp;

            object[] arguments =
                new object[] { point.timestamp, lat, lon, feet, point.x, point.y, point.z, point.xFiltered, point.yFiltered, point.zFiltered, mph, bearing, point.knots, kts, meters};
            string format =
                "{0:HH:mm:ss.fff},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{0:MM/dd/yy}";
            string result = string.Format(format, arguments);
            return result;
        }
    }
}

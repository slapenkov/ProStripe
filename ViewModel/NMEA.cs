using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProStripe.ViewModel
{
	public class NMEA
	{
        public DateTime timestamp { get; private set; }
        public DateTime date    { get; private set; }
		public double latitude { get; private set; }
		public double longitude{ get; private set; }
		public double knots    { get; private set; }
		public double course   { get; private set; }
        public string NS       { get; private set; }
        public string EW       { get; private set; }
        private string valid    { get; set; }
        public double x        { get; private set; }
        public double y        { get; private set; }
        public double z        { get; private set; }
       
    


        private Location where { get; set; }
        private DateTime when { get; set; }
        
        public string sentence { get; private set; }
        public string csv { get; private set; }
        public const string csvHeading =
            "Time,Latitude,Longitude,Altitude,X,Y,Z,temp,cHeading,cKnots,Date";// "Time,Latitude,Longitude,Altitude,X,Y,Z,temp,cHeading,temp,cKnots,Date";
        public double altitude = 0;

		private double Decimal(Timecode t)
		{
            double result;
            result =
                t.user1 * .001 +
                t.user2 * .01 +
                t.user3 * .1 +
                t.user4 +
                t.user5 * 10 +
                t.user6 * 100 +
                t.user7 * 1000;

            return result;
		}

        private bool isStuffed(Timecode t)
        {
            return t.user5 >= 6;
        }

        private double StuffedDecimal(Timecode t)
        {
            double result = Decimal(t);
            if (isStuffed(t))
                result -= 60;
            return result;
        }

        private string Valid(Timecode t)
        {
            if (isStuffed(t))
                return "A";
            return "V";
        }

        private DateTime Timestamp(Timecode t)
        {
            DateTime result;
                result =
                    new DateTime(t.year + 2000, t.month, t.day, t.hour, t.minute, t.second, 0);
            return result;
        }

        private byte Checksum(string s)
        {
            byte result = 0;
            for (int i = 0; i < s.Length; i++)
                result ^= Convert.ToByte(s[i]);
            return result;
        }

        
        // $GPRMC,175032.000,A,3845.4745,N,07604.7669,W,0.01,256.75,081213,,,D*73
        private string formatGPRMC()
        {
            string gprmc;
            object[] arguments =
                new object[] { timestamp, valid, latitude, NS, longitude, EW, knots, course };
            string format =
                "GPRMC,{0:HHmmss.fff},{1},{2:0000.000},{3},{4:00000.000},{5},{6:###0.00},{7:000.00},{0:ddMMyy},,,D";
            gprmc = string.Format(format, arguments);
            byte checksum = Checksum(gprmc);
            return "$" + gprmc + string.Format("*{0:X2}", checksum);
        }

        private string formatCSV()
        {

            double minutes = latitude % 100;
            double lat = (latitude - minutes) / 100 + minutes / 60;
            lat = (NS == "S" ? -lat : lat);
            minutes = longitude % 100;
            double lon = (longitude - minutes) / 100 + minutes / 60;
            lon = (EW == "W" ? -lon : lon);
            double mph = knots;
            double meters = 0;
            double bearing = 0;
            double kts = 0;
            double feet = course; // course * 3.28084 to convert meters to feet 
            
            Location here = new Location(lat, lon);
            if (where != null)
            {
                meters = where.distance(here);
                bearing = where.bearing(here);
                TimeSpan elapsed = timestamp.Subtract(when);
                double ms = elapsed.TotalMilliseconds;
                if (ms > 0)
                    kts = meters * 1000 / ms / 1852;
            }
            where = here;
            when = timestamp;

            object[] arguments =
                new object[] { timestamp, lat, lon, feet, x, y, z, mph, bearing, knots, kts, meters };
            string format =
                "{0:HH:mm:ss.fff},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{0:MM/dd/yy}";
            string result = string.Format(format, arguments);
            return result;
        }

        public bool Decode(Timecode t)
        {
            switch (t.user8)
            {
                case 0:
                    try
                    { timestamp = Timestamp(t); }
                    catch { }
                    break;
                case 8:
                    latitude = StuffedDecimal(t);
                    valid = Valid(t);
                    NS = "N";
                    break;
                case 9:
                    latitude = StuffedDecimal(t);
                    valid = Valid(t);
                    NS = "S";
                    break;
                case 10:
                    longitude = StuffedDecimal(t);
                    if (isStuffed(t))
                        longitude += 10000;
                    EW = "E";
                    break;
                case 11:
                    longitude = StuffedDecimal(t);
                    if (isStuffed(t))
                        longitude += 10000;
                    EW = "W";                    
                    break;
                case 12:  //temperature ::readTemperatureF(void) return 108.5 + (float)readTemperatureRaw() / 480 * 1.8;
                    if (timestamp == new DateTime(2013, 12, 15, 14, 28, 20))
                    { }
                    int tl = t.user6 * 16 + t.user7;
                    int th = t.user5;
                    int rawtmp = (th << 8 | tl);
                    //if (t.user4 > 9)
                      //rawtmp -= 47200;
                    //double tempF = (108.5 + rawtmp) / 864;
                    knots = rawtmp;
                    break;
                case 13: //altitude
                    /*int pxl = t.user6 * 16 + t.user7;
                    int pl = t.user4 * 16 + t.user5;
                    int ph = t.user2 * 16 + t.user3;
                    int rawalt = ph << 16 | pl << 8 | pxl;
                    double pressure_inHg = (rawalt / 138706.5);
                    double altimeter_setting_inHg = 29.9213;
                    double altitudemath = (1 - (Math.Pow((pressure_inHg / altimeter_setting_inHg), 0.190263)));
                    double altitude = (altitudemath * 145442);
                    course = altitude;*/
                    course = Decimal(t);
                    break;
                case 14:  //x and y
                    int xl = t.user6 * 16 + t.user7;
                    int xh = t.user5;
                    int xi = (xh << 8) | xl;

                    //12 bit complement code correction
                    if ((xi & 2048)>0){
                        xi = -((xi-1) ^ 4095);
                    }

                    //if (xh > 191)
                      //  xi -= 65536;
       
                    int xMin = -16240;
                    int xMax = 15616;
                    int newMin = -1000;
                    int newMax = 1000; 
                    double NewRange = (newMax - newMin);
                    double xOldRange = (xMax - xMin);
                    double xRaw = (((xi - xMin) * NewRange) / xOldRange) + newMin;
                    double xAccel = xRaw / 1000.0;
                    x = xi; //Accel;// * .0078;

                    int yl = t.user3 * 16 + t.user4;
                    int yh = t.user1 * 16 + t.user2;
                    int yi = (yh << 8) | yl;
                    if (yh > 191)
                        yi -= 65536; 

                    double ScaledYAxisG = yi * 0.0078d;

                    /*int yMin = -17216;
                    int yMax = 16992;
                    double yOldRange = (yMax - yMin);
                    double yRaw = (((yi - yMin) * NewRange) / yOldRange) + newMin;
                    double yAccel = yRaw / 1000.0;*/
                    y = ScaledYAxisG * 0.0078;//Accel;// * .0078;
                    break;
                case 15: // z
                    int zl = t.user6 * 16 + t.user7;
                    int zh = t.user4 * 16 + t.user5;
                    int zi = (zh << 8) | zl;
                    if (zh > 191)
                        zi -= 65536;

                    /// The scaling to use if the device is not in Full Resolution mode and has a Range of 2g.

                    double ScaledZAxisG = zi * 0.0078d;
                    //double zAxisRadians = Math.Asin(ScaledZAxisG);
                    
                    //LowPassFilter zFilter = new LowPassFilter(0.05); // Create a filter for the X axis.

                    //zFilter.Update(zAxisRadians); // Update the Y axis filter.

                    /*int zMin = -14560;
                    int zMax = 18448;
                    int znewMin = -1000;
                    int znewMax = 1000; 
                    double zNewRange = (znewMax - znewMin);
                    double zOldRange = (zMax - zMin);
                    double zRaw = (((zi - zMin) * zNewRange) / zOldRange) + znewMin;
                    double zAccel = zRaw / 1000.0;
                    */

               

                    z = ScaledZAxisG;//zFilter.Value;//Accel;//* .0078;
                    int ms =
                        t.user3 * 100 + t.user2 * 10 + t.user1;
                    if (timestamp.Year < 2000)
                        return false;
                    timestamp = timestamp.AddMilliseconds(ms);
                    sentence = formatGPRMC();
                    csv = formatCSV();
                    return true;
            }
            return false;
                /*case 14:
                    int xla = t.user6 * 16 + t.user7;
                    int xha = t.user4 * 16 + t.user5;
                    x = (xha<<8)| xla;
                    
                    if (xha > 191)
                        x -= 65536;
                     //x = x * .0078;
                    break;

                case 15:
                    int yla = t.user6 * 16 + t.user7;
                    int yha = t.user4 * 16 + t.user5;
                    y = (yha << 8) | yla;

                    if (yha > 191)
                        y -= 65536;

                    //y = y * .0078;
                    break;
                 

                case 7:
                    int zla = t.user6 * 16 + t.user7;
                    int zha = t.user4 * 16 + t.user5;
                    z = (zha << 8) | zla;

                    //if (zha > 191)
                        //z -= 65536;
                    z = 12345;

                    //y = y * .0078;
                   

                    int ms =
                        t.user3 * 100 + t.user2 * 10 + t.user1;
                    if (timestamp.Year < 2000)
                        return false;
                    timestamp = timestamp.AddMilliseconds(ms);
                    sentence = formatGPRMC();
                    csv = formatCSV();
                    return true;
            }
            return false;*/
        }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProStripe.ViewModel
{
    public class Filter
    {
        public double[] cleanData { get; set; }

        private double[] xNoisy;
        private int p1;
        private double p2;

        public Filter(double[] noisyData, int range, double decay)
        {
            cleanData = new double[noisyData.Length];
            double[] coefficients = Coefficients(range, decay);

            // Calculate divisor value.
            double divisor = 0;
            for (int i = -range; i <= range; i++)
                divisor += coefficients[Math.Abs(i)];

            // Clean main data.
            for (int i = range; i < cleanData.Length - range; i++)
            {
                double temp = 0;
                for (int j = -range; j <= range; j++)
                    temp += noisyData[i + j] * coefficients[Math.Abs(j)];
                cleanData[i] = temp / divisor;
            }

            // Calculate leading and trailing slopes.
            double leadSum = 0;
            double trailSum = 0;
            int leadRef = range;
            int trailRef = cleanData.Length - range - 1;
            for (int i = 1; i <= range; i++)
            {
                leadSum += (cleanData[leadRef] - cleanData[leadRef + i]) / i;
                trailSum += (cleanData[trailRef] - cleanData[trailRef - i]) / i;
            }
            double leadSlope = leadSum / range;
            double trailSlope = trailSum / range;

            // Clean edges.
            for (int i = 1; i <= range; i++)
            {
                cleanData[leadRef - i] = cleanData[leadRef] + leadSlope * i;
                cleanData[trailRef + i] = cleanData[trailRef] + trailSlope * i;
            }
        }

        static private double[] Coefficients(int range, double decay)
        {
            // Precalculate coefficients.
            double[] coefficients = new double[range + 1];
            for (int i = 0; i <= range; i++)
                coefficients[i] = Math.Pow(decay, i);
            return coefficients;
        }
    }
}

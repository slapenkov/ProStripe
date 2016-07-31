using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ProStripe.ViewModel
{
    public class LowPass
{
    static void main(string[] args)
    {
        int range = 5; // Number of data points each side to sample.
        double decay = 0.8; // [0.0 - 1.0] How slowly to decay from raw value.
        double[] noisy = NoisySine();
        double[] clean = CleanData(noisy, range, decay);
        WriteFile(noisy, clean);
    }
 
    static private double[] CleanData(double[] noisy, int range, double decay)
    {
        double[] clean = new double[noisy.Length];
        double[] coefficients = Coefficients(range, decay);
 
        // Calculate divisor value.
        double divisor = 0;
        for (int i = -range; i <= range; i++)
            divisor += coefficients[Math.Abs(i)];
 
        // Clean main data.
        for (int i = range; i < clean.Length - range; i++)
        {
            double temp = 0;
            for (int j = -range; j <= range; j++)
                temp += noisy[i + j] * coefficients[Math.Abs(j)];
            clean[i] = temp / divisor;
        }
 
        // Calculate leading and trailing slopes.
        double leadSum = 0;
        double trailSum = 0;
        int leadRef = range;
        int trailRef = clean.Length - range - 1;
        for (int i = 1; i <= range; i++)
        {
            leadSum += (clean[leadRef] - clean[leadRef + i]) / i;
            trailSum += (clean[trailRef] - clean[trailRef - i]) / i;
        }
        double leadSlope = leadSum / range;
        double trailSlope = trailSum / range;
 
        // Clean edges.
        for (int i = 1; i <= range; i++)
        {
            clean[leadRef - i] = clean[leadRef] + leadSlope * i;
            clean[trailRef + i] = clean[trailRef] + trailSlope * i;
        }
        return clean;
    }
 
    static private double[] Coefficients(int range, double decay)
    {
        // Precalculate coefficients.
        double[] coefficients = new double[range + 1];
        for (int i = 0; i <= range; i++)
            coefficients[i] = Math.Pow(decay, i);
        return coefficients;
    }
 
    static private void WriteFile(double[] noisy, double[] clean)
    {
        using (TextWriter tw = new StreamWriter("data.csv"))
        {
            for (int i = 0; i < noisy.Length; i++)
                tw.WriteLine(string.Format("{0:0.00}, {1:0.00}", noisy[i], clean[i]));
            tw.Close();
        }
    }
 
    static private double[] NoisySine()
    {
        // Create a noisy sine wave.
        double[] noisySine = new double[180];
        Random rnd = new Random();
        for (int i = 0; i < 180; i++)
            noisySine[i] = Math.Sin(Math.PI * i / 90) + rnd.NextDouble() - 0.5;
        return noisySine;
    }
}

    
    }


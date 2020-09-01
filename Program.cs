using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Globalization;


namespace Lua
{
    class Program
    {
        //private static double CandleInterpolation(double sx, double sy, double sx2, double sxy, double[] arr, int n);

        static void Main(string[] args)
        {
            //string pathOpen = @"C:\Projects\Lua\Data\dataOpen.txt";
            //string pathClose = @"C:\Projects\Lua\Data\dataClose.txt";
            //string pathVolume = @"C:\Projects\Lua\Data\dataVolume.txt";
            string pathHigh = @"C:\Projects\Lua\Data\dataHigh.txt";
            string pathLow = @"C:\Projects\Lua\Data\dataLow.txt";

            string[] readHeights = File.ReadAllLines(pathHigh);
            string[] readLows = File.ReadAllLines(pathLow);

            double[] arrHigh = new double[readHeights.Length];
            double[] arrLow = new double[readLows.Length];
            for (int i = 0; i < readHeights.Length; i++) //readHeights.Length = readLows.Length
            {
                arrHigh[i] = double.Parse(readHeights[i], CultureInfo.InvariantCulture);
                arrLow[i] = double.Parse(readLows[i], CultureInfo.InvariantCulture);
            }

            double globalMin = double.PositiveInfinity;  // Значение гМина
            double globalMax = double.NegativeInfinity; // Значение гмакса
            int idxMin = 0;                               // Индекс найденного глобального минимума
            int idxMax = 0;                               // Индекс найденного глобального максимума
            double heightAvg = 0;
            double lowAvg = 0;
            for (int i = arrLow.Length - 1; i > 0; i--)
            {
                lowAvg += arrLow[i];
                if (globalMin > arrLow[i])
                {
                    globalMin = arrLow[i];
                    idxMin = i;
                }
            }
            for (int i = arrHigh.Length - 1; i > 0; i--)
            {
                heightAvg += arrHigh[i];
                if (globalMax < arrHigh[i])
                {
                    globalMax = arrHigh[i];
                    idxMax = i;
                }
            }

            lowAvg /= arrLow.Length;
            heightAvg /= arrHigh.Length;
            if ((globalMax - globalMin) < (0.005 * globalMax))
            {
                Console.WriteLine("[Ширина коридора] = {0}\nБоковик слишком узок", globalMax - globalMin);
                return;
            }

            // f(x) = kx + b
            // Нужно найти коэффициент k, стремящийся к 0, при помощи метода линейной интерполяции
            double kHigh = CandleInterpolation(arrHigh, arrHigh.Length);
            double kLow  = CandleInterpolation(arrLow, arrLow.Length);

            Console.WriteLine("[kHigh] = {0}\n[kLow] = {1}", kHigh, kLow);

            Console.WriteLine("[gMin] = {0}\n[gMax] = {1}", globalMin, globalMax);
            Console.WriteLine("[Current AVG] = {0}", heightAvg);
            Console.WriteLine("[arrHigh.Length] = {0}", arrHigh.Length);
            return;
        }

        private static double CandleInterpolation(double[] arr, int n)
        {
            double sx = 0;
            double sy = 0;
            double sx2 = 0;
            double sxy = 0;

            for (int i = 0; i < n - 1; i++)
            {
                sx += i;
                sy += arr[i];
                sx2 += i * 8;
                sxy += i * arr[i];
            }
            return ((n * sxy) - (sx * sy)) / ((n * sx2) - (sx * sx));
        }
    }
}


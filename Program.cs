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

        public struct Candle 
        {
            public double high;
            public double low;
        }

        static void Main(string[] args)
        {
            //string pathOpen = Path.Combine(Directory.GetCurrentDirectory(), @"Data\dataOpen.txt");
            //string pathClose = Path.Combine(Directory.GetCurrentDirectory(), @"Data\dataClose.txt");
            //string pathVolume = Path.Combine(Directory.GetCurrentDirectory(), @"Data\dataVolume.txt");
            string pathHigh = Path.Combine(Directory.GetCurrentDirectory(), @"Data\dataHigh.txt");
            string pathLow  = Path.Combine(Directory.GetCurrentDirectory(), @"Data\dataLow.txt");

            string[] readHeights = File.ReadAllLines(pathHigh);
            string[] readLows    = File.ReadAllLines(pathLow);

            Candle[] candles = new Candle[readHeights.Length];
            for (int i = 0; i < readHeights.Length; i++) //readHeights.Length = readLows.Length
            {
                candles[i].high = double.Parse(readHeights[i], CultureInfo.InvariantCulture);
                candles[i].low  = double.Parse(readLows[i]   , CultureInfo.InvariantCulture);
            }

            double globalMin = double.PositiveInfinity;  // Значение гМина
            double globalMax = double.NegativeInfinity;  // Значение гмакса

            int idxGMin = 0;    // Индекс найденного глобального минимума
            int idxGMax = 0;    // Индекс найденного глобального максимума
            int idxSMin = 0;    // Индекс вторичного минимума
            int idxSMax = 0;    // ИНдекс вторичного максимума

            double heightAvg = 0;
            double lowAvg = 0;

            for (int i = 0; i < candles.Length - 1; i++)
            {
                lowAvg += candles[i].low;
                if (globalMin > candles[i].low)
                {
                    globalMin = candles[i].low;
                    idxGMin = i;
                }
            }
            for (int i = 0; i < candles.Length - 1; i++)
            {
                heightAvg += candles[i].high;
                if (globalMax < candles[i].high)
                {
                    globalMax = candles[i].high;
                    idxGMax = i;
                }
            }

            lowAvg /= candles.Length;
            heightAvg /= candles.Length;
            if ((globalMax - globalMin) < (0.005 * globalMax))
            {
                Console.WriteLine("[gMin] = {0} [{1}]\n[gMax] = {2} [{3}]", globalMin, idxGMin+1, globalMax, idxGMax+1);
                Console.WriteLine("[Ширина коридора] = {0}\nБоковик слишком узок", globalMax - globalMin);
                Console.WriteLine("[0.5% от цены] = {0} у.е.", 0.005 * globalMax);
                Console.WriteLine();
                return;
            }

            // f(x) = kx + b
            // Нужно найти коэффициент k, стремящийся к 0, при помощи метода линейной интерполяции
            var ks = CandleInterpolation(candles);
            Console.WriteLine("[kHigh] = {0}\n[kLow] = {1}", ks.Item1, ks.Item2);
            double k = (ks.Item1 + ks.Item2) * 0.5;
            Console.WriteLine("[k] = {0}", k);
            Console.WriteLine("[gMin] = {0} [{1}]\n[gMax] = {2} [{3}]", globalMin, idxGMin+1, globalMax, idxGMax+1);
            Console.WriteLine("[Current AVG] = {0}", heightAvg);
            Console.WriteLine("[arrHigh.Length] = {0}", candles.Length);

            double kOffset = 0.05;
            if(Math.Abs(k) < kOffset)
            {
                Console.WriteLine("Интерполяционная линия почти горизонтальна. Цена в боковике");
            } else if(k < 0) {
                Console.WriteLine("Интерполяционная линия имеет сильный убывающий тренд");
            } else {
                Console.WriteLine("Интерполяционная линия имеет сильный возрастающий тренд");
            }
            Console.WriteLine();
            return;
        }

        private static (double, double) CandleInterpolation(Candle[] cdls)
        {
            double kHigh = 0;
            double kLow  = 0;

            int n = cdls.Length;

            double sx = 0;
            double sy = 0;
            double sx2 = 0;
            double sxy = 0;

            for (int i = 0; i < n - 1; i++)
            {
                sx += i;
                sy += cdls[i].high;
                sx2 += i * 8;
                sxy += i * cdls[i].high;
            }
            kHigh = ((n * sxy) - (sx * sy)) / ((n * sx2) - (sx * sx));

            sx = 0;
            sy = 0;
            sx2 = 0;
            sxy = 0;

            for (int i = 0; i < n - 1; i++)
            {
                sx += i;
                sy += cdls[i].low;
                sx2 += i * 8;
                sxy += i * cdls[i].low;
            }
            kLow = ((n * sxy) - (sx * sy)) / ((n * sx2) - (sx * sx));

            return (kHigh, kLow);
        }
    }
}


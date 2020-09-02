using System;
using System.IO;
using System.Globalization;


namespace Lua
{
    class Program
    {
        /// <summary>
        /// Структура свечи
        /// </summary>
        public struct Candle
        {
            public double high;
            public double low;
            public double close;
        }

        // Минимальная ширина коридора (коэфф. от цены инструмента)
        public const double minWidthCoeff = 0.005;

        static void Main(string[] args)
        {
            //string pathOpen = Path.Combine(Directory.GetCurrentDirectory(), @"Data\dataOpen.txt");
            //string pathVolume = Path.Combine(Directory.GetCurrentDirectory(), @"Data\dataVolume.txt");
            string pathClose = Path.Combine(Directory.GetCurrentDirectory(), @"Data\dataClose.txt");
            string pathHigh = Path.Combine(Directory.GetCurrentDirectory(), @"Data\dataHigh.txt");
            string pathLow = Path.Combine(Directory.GetCurrentDirectory(), @"Data\dataLow.txt");

            string[] readHeights = File.ReadAllLines(pathHigh);
            string[] readLows = File.ReadAllLines(pathLow);
            string[] readCloses = File.ReadAllLines(pathClose);

            Candle[] candles = new Candle[readHeights.Length];
            for (int i = 0; i < readHeights.Length; i++) //readHeights.Length = readLows.Length
            {
                candles[i].high = double.Parse(readHeights[i], CultureInfo.InvariantCulture);
                candles[i].low = double.Parse(readLows[i], CultureInfo.InvariantCulture);
                candles[i].close = double.Parse(readCloses[i], CultureInfo.InvariantCulture);
            }

            var lowInfo  = GlobalExtremumsAndMA(candles, false); // Среднее по лоу всего графика
            var highInfo = GlobalExtremumsAndMA(candles, true);  // Среднее по хаям всего графика
            Console.WriteLine(lowInfo);
            Console.WriteLine(highInfo);

            double globalMin = lowInfo.Item1;   // Значение гМина
            double globalMax = highInfo.Item1;  // Значение гмакса
            int idxGMin = lowInfo.Item2;     // Индекс найденного глобального минимума
            int idxGMax = highInfo.Item2;    // Индекс найденного глобального максимума
            double lowMA = lowInfo.Item3;
            double highMA = highInfo.Item3;
            double MA = (highMA + lowMA) * 0.5;

            // f(x) = kx + b
            // Нужно найти коэффициент k, стремящийся к 0, при помощи метода линейной интерполяции
            var ks = FindKs(candles);
            double k = (ks.Item1 + ks.Item2) * 0.5;
            double kOffset = 0.05;

            PrintInfo(globalMin, globalMax, idxGMin, idxGMax, ks, k, kOffset, candles, MA);

            return;
        }

        /// <summary>
        /// Функция поиска глобальных экстремумов в массиве структур свечей
        /// </summary>
        /// <param name="cdls">Массив структур свечей</param>
        /// <param name="onHigh">true - ищем по high. false - по low</param>
        /// <returns></returns>
        private static (double, int, double) GlobalExtremumsAndMA(Candle[] cdls, bool onHigh)
        {
            // Значение, среднее значение и индекс искомого глобального экстремума
            double globalExtremum = 0;
            double MA = 0;
            int index = 0;

            if (onHigh)
            {
                globalExtremum = double.NegativeInfinity;
                for (int i = 0; i < cdls.Length; i++)
                {
                    MA += cdls[i].high;
                    if (globalExtremum < cdls[i].high)
                    {
                        globalExtremum = cdls[i].high;
                        index = i;
                    }
                }
            }
            else
            {
                globalExtremum = double.PositiveInfinity;
                for (int i = 0; i < cdls.Length; i++)
                {
                    MA += cdls[i].low;
                    if (globalExtremum > cdls[i].low)
                    {
                        globalExtremum = cdls[i].low;
                        index = i;
                    }
                }
            }
            MA /= cdls.Length;

            return (globalExtremum, index, MA);
        }

        /// <summary>
        /// Функция поиска угла наклона аппроксимирующей прямой
        /// </summary>
        /// <param name="cdls">Массив структур свечей</param>
        /// <returns>Углы наклона аппроксимирующих прямых по high и по low</returns>
        private static (double, double) FindKs(Candle[] cdls)
        {
            double kHigh = 0;
            double kLow = 0;

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

        private static void PrintInfo(double gMin, double gMax, int iGMin, int iGMax, (double, double) ks, double k,
                                        double kOff, Candle[] cdls, double movAvg)
        {
            Console.WriteLine("[gMin] = {0} [{1}]\n[gMax] = {2} [{3}]", gMin, iGMin + 1, gMax, iGMax + 1);
            Console.WriteLine("[kHigh] = {0}\n[kLow] = {1}", ks.Item1, ks.Item2);
            Console.WriteLine("[k] = {0}", k);
            Console.WriteLine("[Скользаящая средняя] = {0}", movAvg);
            Console.WriteLine("[arrHigh.Length] = {0}", cdls.Length);

            if ((gMax - gMin) < (minWidthCoeff * movAvg))
            {
                Console.WriteLine("[Ширина коридора] = {0}\nБоковик слишком узок", gMax - gMin);
                Console.WriteLine("[Минимальная ширина коридора] = {0} у.е.", minWidthCoeff * movAvg);
            }
            else
            {
                if (Math.Abs(k) < kOff)
                {
                    Console.WriteLine("Интерполяционная линия почти горизонтальна. Цена потенциально в боковике");
                }
                else if (k < 0)
                {
                    Console.WriteLine("Интерполяционная линия имеет сильный убывающий тренд");
                }
                else
                {
                    Console.WriteLine("Интерполяционная линия имеет сильный возрастающий тренд");
                }
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Функция находит среднеквадратическое отклонение свечей внутри коридора
        /// </summary>
        /// <param name="cdls">Массив структур свечей</param>
        /// <param name="minWidth">Минимальная ширина коридора для боковика</param>
        /// <param name="movAvg">Скользящая средняя</param>
        /// <param name="widthCoeff">Коэффициент для минимальной ширины коридора</param>
        /// <returns>Среднеквадратические отклоенения по high и по low соответственно</returns>
        private static (double, double) StandartDeviation(Candle[] cdls, double minWidth, double movAvg,
                                                          double widthCoeff)
        {
            double sumHigh = 0;
            double sumLow = 0;
            for (int i = 0; i < cdls.Length - 1; i++)
            {
                sumHigh += Math.Pow((cdls[i].high - movAvg), 2);
                sumLow += Math.Pow(cdls[i].low - movAvg, 2);
            }
            double SDHigh = Math.Sqrt(sumHigh / cdls.Length);
            double SDLow = Math.Sqrt(sumLow / cdls.Length);
            return (SDHigh, SDLow);
        }
    }
}


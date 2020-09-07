using System;
using System.IO;
using System.Globalization;

namespace Lua
{
    class Program
    {
        /// <summary>
        /// Структура свечи (high, low, close...)
        /// </summary>
        public struct Candle
        {
            /// <value> Хай текущей свечи </value>
            public double high;
            /// <value> Лоу текущей свечи </value>
            public double low;
            /// <value> Цена закрытия текущей свечи </value>
            public double close;

           /// <value> Средняя цена по свече (= (хай - лоу)/*0.5) </value>
            public double avg;
        }

        /// <summary>
        /// Минимальная ширина коридора (коэфф. от цены инструмента)
        /// </summary>
        public const double minWidthCoeff = 0.005;

        /// <summary>
        /// Коэффициент для определения поведения тренда
        /// </summary>
        public const double kOffset = 0.05;

        /// <summary>
        /// Возможное отклонение экстремума от линии СКО (коэфф * цену)
        /// </summary>
        public const double SDOffset = 0.00025;


        static void Main(string[] args)
        {
            //string pathOpen = Path.Combine(Directory.GetCurrentDirectory(), @"Data\dataOpen.txt");
            //string pathVolume = Path.Combine(Directory.GetCurrentDirectory(), @"Data\dataVolume.txt");
            string pathClose = Path.Combine(Directory.GetCurrentDirectory(), @"Data\dataClose.txt");
            string pathHigh  = Path.Combine(Directory.GetCurrentDirectory(), @"Data\dataHigh.txt");
            string pathLow   = Path.Combine(Directory.GetCurrentDirectory(), @"Data\dataLow.txt");
            string pathAvg   = Path.Combine(Directory.GetCurrentDirectory(), @"Data\dataAvg.txt");

            string[] readHeights = File.ReadAllLines(pathHigh);
            string[] readLows    = File.ReadAllLines(pathLow);
            string[] readCloses  = File.ReadAllLines(pathClose);
            string[] readAvgs    = File.ReadAllLines(pathAvg);

            Candle[] candles = new Candle[readHeights.Length];
            for (int i = 0; i < readHeights.Length; i++) //readHeights.Length = readLows.Length
            {
                candles[i].high  = double.Parse(readHeights[i], CultureInfo.InvariantCulture);
                candles[i].low   = double.Parse(readLows[i]   , CultureInfo.InvariantCulture);
                candles[i].close = double.Parse(readCloses[i] , CultureInfo.InvariantCulture);
                candles[i].avg   = double.Parse(readAvgs[i]   , CultureInfo.InvariantCulture);
            }

            var lowInfo = GlobalExtremumsAndMA(candles, false); // Среднее по лоу всего графика
            var highInfo = GlobalExtremumsAndMA(candles, true);  // Среднее по хаям всего графика

            double globalMin = lowInfo.Item1;   // Значение гМина
            double globalMax = highInfo.Item1;  // Значение гмакса
            int idxGMin = lowInfo.Item2;     // Индекс найденного глобального минимума
            int idxGMax = highInfo.Item2;    // Индекс найденного глобального максимума
            double lowMA = lowInfo.Item3;
            double highMA = highInfo.Item3;

            double MA = (highMA + lowMA) * 0.5; // Скользящая средняя 

            // f(x) = kx + b
            // Нужно найти коэффициент k, стремящийся к 0, при помощи метода линейной интерполяции
            double k = FindK(candles);

            // Нашли среднеквадратичесоке отклонение всех high выше avg
            // и всех low ниже avg
            var SD = StandartDeviation(candles, MA, minWidthCoeff);
            double SDL = SD.Item1;
            double SDH = SD.Item2;

            // TODO: Функция, определяющая, находится ли текущий локальный экстремум
            // (возожно, аннулировав по 3 свечи слева и справа от себя) 
            // достаточно близко к линии СКО
            int extremumsNearSDL = ExtremumsNearSD(candles, MA, SDL, false);
            int extremumsNearSDH = ExtremumsNearSD(candles, MA, SDH, true);

            PrintInfo(globalMin, globalMax, idxGMin, idxGMax, k, candles, MA,
            SDH, SDL, extremumsNearSDL, extremumsNearSDH);

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
        private static double FindK(Candle[] cdls)
        {
            double k = 0;

            int n = cdls.Length;

            double sx = 0;
            double sy = 0;
            double sx2 = 0;
            double sxy = 0;

            for (int i = 0; i < n; i++)
            {
                sx += i;
                sy += cdls[i].avg;
                sx2 += i * 8;
                sxy += i * cdls[i].avg;
            }
            k = ((n * sxy) - (sx * sy)) / ((n * sx2) - (sx * sx));

            return k;
        }

        private static void PrintInfo(double gMin, double gMax, int iGMin, int iGMax, double k,
                                    Candle[] cdls, double movAvg, double SDH, double SDL, int exsNearSDL, int exsNearSDH)
        {
            Console.WriteLine("[gMin] = {0} [{1}]\t[gMax] = {2} [{3}]", gMin, iGMin + 1, gMax, iGMax + 1);
            Console.WriteLine("[k] = {0}", k);
            Console.WriteLine("[Скользаящая средняя] = {0}", movAvg);
            Console.WriteLine("[candles.Length] = {0}", cdls.Length);
            Console.WriteLine("[SDL] = {0}  [SDH] = {1}", SDL, SDH);
            Console.WriteLine("[Экстремумы рядом с СКО low] = {0}\t[Экстремумы рядом с СКО high] = {1}", exsNearSDL, exsNearSDH);

            if ((gMax - gMin) < (minWidthCoeff * movAvg))
            {
                Console.Write("[Ширина коридора] = {0}\nБоковик слишком узок!\t", gMax - gMin);
                Console.Write("[Минимальная ширина коридора] = {0} у.е.\n", minWidthCoeff * movAvg);
            }

            if (Math.Abs(k) < kOffset)
            {
                Console.WriteLine("Аппрокимирующая линия почти горизонтальна. Цена потенциально в боковике");
            }
            else if (k < 0)
            {
                Console.WriteLine("Аппроксимирующая линия имеет сильный убывающий тренд");
            }
            else
            {
                Console.WriteLine("Аппроксимирующая линия имеет сильный возрастающий тренд");
            }


            Console.WriteLine();
        }

        /// <summary>
        /// Функция находит среднеквадратическое отклонение свечей тех, что выше среднего, 
        /// и тех, что ниже внутри коридора
        /// </summary>
        /// <param name="cdls">Массив структур свечей</param>
        /// <param name="minWidth">Минимальная ширина коридора для боковика</param>
        /// <param name="movAvg">Скользящая средняя</param>
        /// <param name="widthCoeff">Коэффициент для минимальной ширины коридора</param>
        /// <returns>Среднеквадратические отклоенения по high и по low соответственно</returns>
        private static (double, double) StandartDeviation(Candle[] cdls, double movAvg, double widthCoeff)
        {
            double sumLow = 0;
            double sumHigh = 0;

            // Количество low и high, находящихся шире минимальной ширины коридора
            int lowsCount = 0;
            int highsCount = 0;

            for (int i = 0; i < cdls.Length - 1; i++)
            {
                if ((cdls[i].low) <= (movAvg - kOffset))
                {
                    sumLow += Math.Pow(movAvg - cdls[i].low, 2);
                    lowsCount++;
                }
                else if ((cdls[i].high) >= (movAvg + kOffset))
                {
                    sumHigh += Math.Pow(cdls[i].high - movAvg, 2);
                    highsCount++;
                }
            }
            double SDLow = Math.Sqrt(sumLow / lowsCount);
            double SDHigh = Math.Sqrt(sumHigh / highsCount);

            return (movAvg - SDLow, SDHigh + movAvg);
        }

        /// <summary>
        /// Функция, подсчитывающая количество экстремумов, находящихся поблизости СКО
        /// </summary>
        /// <param name="cdls">Массив свечей</param>
        /// <param name="movAvg">Средняя цена</param>
        /// <param name="standartDeviation">СКО</param>
        /// <param name="onHigh">Ищем по хаям или по лоу</param>
        /// <returns>Количество свечей возле значения СКО (оффсет = SDOffset)</returns>
        private static int ExtremumsNearSD(Candle[] cdls, double movAvg, double standartDeviation, bool onHigh)
        {
            int extremums = 0;
            double rangeToReachSD = movAvg * SDOffset;

            if (!onHigh)
            {
                Console.WriteLine("[Попавшие в low индексы]:");
                for (int i = cdls.Length - 3; i > 1; i--) // Кажется, здесь есть проблема индексаций Lua и C#
                {
                    if ((Math.Abs(cdls[i].low - standartDeviation) <= (rangeToReachSD)) &&
                        (cdls[i].low <= cdls[i-1].low) &&
                        (cdls[i].low <= cdls[i-2].low) &&
                        (cdls[i].low <= cdls[i+1].low) &&
                        (cdls[i].low <= cdls[i+2].low))
                    {
                        Console.Write("{0}({1}) ", cdls[i].low, i + 1);
                        extremums++;
                        cdls[i].low -= 0.01; // Костыль, чтобы следующая(соседняя) свеча более вероятно не подошла
                    }
                }
                Console.WriteLine("\n[rangeToReachSD] =  {0}", rangeToReachSD);
                Console.WriteLine("[rangeToReachSD + standartDeviation] = {0}", rangeToReachSD + standartDeviation);
            }
            else
            {
                Console.WriteLine("[Попавшие в high индексы]:");
                for (int i = cdls.Length - 3; i > 1; i--)
                {
                    if ((Math.Abs(cdls[i].high - standartDeviation) <= (rangeToReachSD)) &&
                        (cdls[i].high >= cdls[i-1].high) &&
                        (cdls[i].high >= cdls[i-2].high) &&
                        (cdls[i].high >= cdls[i+1].high) &&
                        (cdls[i].high >= cdls[i+2].high))
                    {
                        Console.Write("{0}({1}) ", cdls[i].high, i + 1);
                        extremums++;
                        cdls[i].high += 0.01;
                    }
                }
                
                Console.WriteLine("\n[rangeToReachSD] =  {0}", rangeToReachSD);
                Console.WriteLine("[rangeToReachSD + standartDeviation] = {0}", rangeToReachSD + standartDeviation);
            }

            return extremums;
        }
    }
}


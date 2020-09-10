using System;
using System.IO;
using System.Globalization;

namespace Lua
{    class Program
    {
        static void Main(string[] args)
        {
            Reader reader = new Reader();
            reader.GetAllData();

            _CandleStruct[] candles = new _CandleStruct[reader.readHeights.Length];
            for (int i = 0; i < reader.readHeights.Length; i++) //readHeights.Length = readLows.Length
            {
                candles[i].high  = double.Parse(reader.readHeights[i], CultureInfo.InvariantCulture);
                candles[i].low   = double.Parse(reader.readLows[i]   , CultureInfo.InvariantCulture);
                candles[i].close = double.Parse(reader.readCloses[i] , CultureInfo.InvariantCulture);
                candles[i].avg   = double.Parse(reader.readAvgs[i]   , CultureInfo.InvariantCulture);
            }
            
            FlatIdentifier flatIdentifier = new FlatIdentifier(candles);

            double globalMin = flatIdentifier.lowInfo.Item1;    // Значение гМина
            double globalMax = flatIdentifier.highInfo.Item1;   // Значение гМакса
            int idxGMin = flatIdentifier.lowInfo.Item2;         // Индекс найденного глобального минимума
            int idxGMax = flatIdentifier.highInfo.Item2;        // Индекс найденного глобального максимума
            double lowMA  = flatIdentifier.lowInfo.Item3;       // Среднее по лоу
            double highMA = flatIdentifier.highInfo.Item3;      // Среднее по хай

            double MA = (highMA + lowMA) * 0.5; // Скользящая средняя 

            // f(x) = kx + b
            // Нужно найти коэффициент k, стремящийся к 0, при помощи метода линейной аппроксимации
            double k = flatIdentifier.FindK();

            // Нашли среднеквадратическое отклонение всех high выше avg и всех low ниже avg
            var SD = flatIdentifier.StandartDeviation(MA);
            double SDL = SD.Item1;
            double SDH = SD.Item2;

            int extremumsNearSDL = flatIdentifier.ExtremumsNearSD(MA, SDL, false);
            int extremumsNearSDH = flatIdentifier.ExtremumsNearSD(MA, SDH, true);

            Printer printer = new Printer(candles, globalMin, globalMax, idxGMin, idxGMax, MA, k, SDL, SDH, extremumsNearSDL, extremumsNearSDH);
            printer.OutputInfo();

            return;
        }
    }
}


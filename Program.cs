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

            var lowInfo   = flatIdentifier.GlobalExtremumsAndMA(false); // Среднее по лоу всего графика
            var highInfo  = flatIdentifier.GlobalExtremumsAndMA(true);  // Среднее по хай всего графика

            double globalMin = lowInfo.Item1;   // Значение гМина
            double globalMax = highInfo.Item1;  // Значение гМакса
            int idxGMin = lowInfo.Item2;     // Индекс найденного глобального минимума
            int idxGMax = highInfo.Item2;    // Индекс найденного глобального максимума
            double lowMA  = lowInfo.Item3;
            double highMA = highInfo.Item3;

            double MA = (highMA + lowMA) * 0.5; // Скользящая средняя 

            // f(x) = kx + b
            // Нужно найти коэффициент k, стремящийся к 0, при помощи метода линейной аппроксимации
            double k = flatIdentifier.FindK();

            // Нашли среднеквадратичесоке отклонение всех high выше avg
            // и всех low ниже avg
            var SD = flatIdentifier.StandartDeviation(MA);
            double SDL = SD.Item1;
            double SDH = SD.Item2;

            // TODO: Функция, определяющая, находится ли текущий локальный экстремум
            // (возожно, аннулировав по 3 свечи слева и справа от себя) 
            // достаточно близко к линии СКО
            int extremumsNearSDL = flatIdentifier.ExtremumsNearSD(MA, SDL, false);
            int extremumsNearSDH = flatIdentifier.ExtremumsNearSD(MA, SDH, true);

            //PrintInfo(globalMin, globalMax, idxGMin, idxGMax, k, candles, MA,
            //SDH, SDL, extremumsNearSDL, extremumsNearSDH);
            Printer printer = new Printer(candles, globalMin, globalMax, idxGMin, idxGMax, MA, k, SDL, SDH, extremumsNearSDL, extremumsNearSDH);
            printer.OutputInfo();

            return;
        }
    }
}


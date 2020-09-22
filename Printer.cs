using System;
// ReSharper disable StringLiteralTypo

namespace Lua
{
    class Printer
    {
        private FlatIdentifier fi;
        private HistoricalFlatFinder hFF;
        public Printer() {}
        public Printer(FlatIdentifier _flatIdentifier)
        {
            fi = _flatIdentifier;
        }

        public Printer(HistoricalFlatFinder _historicalFF)
        {
            hFF = _historicalFF;
        }

        public void OutputApertureInfo()
        {
            Console.WriteLine("[gMin] = {0} [{1}]\t[gMax] = {2} [{3}]", fi.GMin, fi.IdxGmin + 1, fi.GMax, fi.IdxGmax + 1);
            Console.WriteLine("[k] = {0}", fi.K);
            Console.WriteLine("[Скользящая средняя] = {0}", fi.MovAvg);
            Console.WriteLine("[candles.Length] = {0}", fi.candles.Count);
            Console.WriteLine("[SDL] = {0}\t\t[SDH] = {1}", fi.SDL, fi.SDH);
            Console.WriteLine("[Экстремумы рядом с СКО low] = {0}\t[Экстремумы рядом с СКО high] = {1}", fi.ExsNearSDL, fi.ExsNearSDH);

            switch (fi.trend)
            {
                case Trend.Down:
                {
                    Console.Write("[Ширина коридора] = {0}\t", fi.flatWidth);
                    Console.Write("[Минимальная ширина коридора] = {0}\n", _Constants.MinWidthCoeff * fi.MovAvg);
                    Console.WriteLine("Аппроксимирующая линия имеет сильный убывающий тренд");
                    break;
                }
                case Trend.Up:
                {
                    Console.Write("[Ширина коридора] = {0}\nБоковик слишком узок!\t", fi.flatWidth);
                    Console.Write("[Минимальная ширина коридора] = {0}\n", _Constants.MinWidthCoeff * fi.MovAvg);
                    Console.WriteLine("Аппроксимирующая линия имеет сильный возрастающий тренд");
                    break;
                }
                case Trend.Neutral:
                {
                    Console.Write("[Ширина коридора] = {0}\t", fi.flatWidth);
                    Console.WriteLine("[Минимальная ширина коридора] = {0}\n", _Constants.MinWidthCoeff * fi.MovAvg);
                    Console.WriteLine("Аппроксимирующая линия почти горизонтальна. Тренд нейтральный");
                    Console.WriteLine("Цена, вероятно, формирует боковик...");
                    break;
                }
                default: 
                    break;
            }

            if ((fi.flatWidth) < (_Constants.MinWidthCoeff * fi.MovAvg))
            {
                Console.WriteLine("Боковик слишком узок!");
            }

            Console.WriteLine();
        }

        public void OutputHistoricalInfo()
        {
            Console.WriteLine("Боковиков найдено: {0}", hFF.FlatsFound);
        }
    }
}

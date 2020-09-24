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
            Console.WriteLine("[Median] = {0}", fi.Median);
            Console.WriteLine("[candles.Count] = {0}", fi.candles.Count);
            Console.WriteLine("[SDL] = {0}\t\t[SDH] = {1}", fi.SDL, fi.SDH);
            Console.WriteLine("[Экстремумы рядом с СКО low] = {0}\t[Экстремумы рядом с СКО high] = {1}", fi.ExsNearSDL, fi.ExsNearSDH);
            Console.WriteLine("[Границы окна]: [{0}]\t[{1}]", fi.FlatBounds.left.date, fi.FlatBounds.right.date);
            
            switch (fi.trend)
            {
                case Trend.Down:
                {
                    Console.Write("[Ширина коридора] = {0}\t", fi.flatWidth);
                    Console.WriteLine("[Минимальная ширина коридора] = {0}", _Constants.MinWidthCoeff * fi.Median);
                    Console.WriteLine("Аппроксимирующая линия имеет сильный убывающий тренд");
                    break;
                }
                case Trend.Up:
                {
                    Console.Write("[Ширина коридора] = {0}\t", fi.flatWidth);
                    Console.WriteLine("[Минимальная ширина коридора] = {0}", _Constants.MinWidthCoeff * fi.Median);
                    Console.WriteLine("Аппроксимирующая линия имеет сильный возрастающий тренд");
                    break;
                }
                case Trend.Neutral:
                {
                    Console.Write("[Ширина коридора] = {0}\t", fi.flatWidth);
                    Console.WriteLine("[Минимальная ширина коридора] = {0}", _Constants.MinWidthCoeff * fi.Median);
                    Console.WriteLine("Аппроксимирующая линия почти горизонтальна. Тренд нейтральный");
                    if (fi.IsFlat)
                    {
                        Console.WriteLine("Цена, вероятно, формирует боковик...");
                    }
                    break;
                }
                default: 
                    break;
            }

            if ((fi.flatWidth) < (_Constants.MinWidthCoeff * fi.Median))
            {
                Console.WriteLine("Боковик слишком узок");
            }

            Console.WriteLine();
        }

        public void WhyIsNotFlat()
        {
            string reason = "";
            Console.WriteLine("Окно с {0} по {1}", fi.FlatBounds.left.date, fi.FlatBounds.right.date);
            Console.WriteLine("В окне не определено боковое движение.\nВозможные причины:");
            
            switch (fi.trend)
            {
                case Trend.Down:
                {
                    reason += "Нисходящий тренд. ";
                    if ((fi.flatWidth) < (_Constants.MinWidthCoeff * fi.Median))
                    {
                        reason += "Недостаточная ширина коридора. ";
                    }

                    if (fi.ExsNearSDL < 2)
                    {
                        reason += "Недостаточно вершин снизу возле СКО.  ";
                    } else if (fi.ExsNearSDH < 2)
                    {
                        reason += "Недостаточно вершин сверху возле СКО. ";
                    }
                    break;
                }
                case Trend.Up:
                {
                    reason += "Восходящий тренд. ";
                    if ((fi.flatWidth) < (_Constants.MinWidthCoeff * fi.Median))
                    {
                        reason += "Недостаточная ширина коридора. ";
                    }

                    if (fi.ExsNearSDL < 2)
                    {
                        reason += "Недостаточно вершин снизу возле СКО.  ";
                    } else if (fi.ExsNearSDH < 2)
                    {
                        reason += "Недостаточно вершин сверху возле СКО. ";
                    }
                    break;
                }
                case Trend.Neutral:
                {
                    if ((fi.flatWidth) < (_Constants.MinWidthCoeff * fi.Median))
                    {
                        reason += "Недостаточная ширина коридора. ";
                    }

                    if (fi.ExsNearSDL < 2)
                    {
                        reason += "Недостаточно вершин снизу возле СКО.  ";
                    } else if (fi.ExsNearSDH < 2)
                    {
                        reason += "Недостаточно вершин сверху возле СКО. ";
                    }
                    break;
                }
            }

            Console.WriteLine(reason);
            Console.WriteLine();
        }

        public void OutputHistoricalInfo()
        {
            Console.WriteLine("Боковиков найдено: {0}", hFF.FlatsFound);
            Console.WriteLine("Боковики определены в: ");
            for (int i = 0; i < hFF.ApertureBounds.Count; i++)
            {
                Console.WriteLine("[{0}]\t[{1}]", hFF.ApertureBounds[i].left.date, hFF.ApertureBounds[i].right.date);
            }
        }
    }
}

using System;
using NLog;
using NLog.Conditions;
using NLog.Config;
using NLog.Targets;
// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo

namespace Lua
{
    class Printer
    {
        // С другой стороны, логгер не должен инициализировать в классе. Это затратно
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        private readonly FlatIdentifier fi;
        private readonly HistoricalFlatFinder hFf;

        private Printer()
        {
            // TODO: Разобраться, насколько неэффективно каждый раз настраивать логгер 
        }
        public Printer(FlatIdentifier flatIdentifier) : this()
        {
            fi = flatIdentifier;
        }

        public Printer(HistoricalFlatFinder historicalFf) : this()
        {
            hFf = historicalFf;
        }

        public void OutputApertureInfo()
        {
            logger.Trace("[gMin] = {0} [{1}]\t[gMax] = {2} [{3}]\n", fi.GMin, fi.IdxGmin + 1, fi.GMax, fi.IdxGmax + 1);
            logger.Trace("[k] = {0}\n", fi.K);
            logger.Trace("[Median] = {0}\n", fi.Median);
            logger.Trace("[candles.Count] = {0}\n", fi.candles.Count);
            logger.Trace("[SDL] = {0}\t\t[SDH] = {1}\n", fi.SDL, fi.SDH);
            logger.Trace("[Экстремумы рядом с СКО low] = {0}\t[Экстремумы рядом с СКО high] = {1}\n", fi.ExsNearSDL, fi.ExsNearSDH);
            logger.Trace("[Границы окна]: [{0}]\t[{1}]\n", fi.FlatBounds.left.date, fi.FlatBounds.right.date);
            
            switch (fi.trend)
            {
                case Trend.Down:
                {
                    logger.Trace("[Ширина коридора] = {0}\t", fi.flatWidth);
                    logger.Trace("[Минимальная ширина коридора] = {0}\n", _Constants.MinWidthCoeff * fi.Median);
                    logger.Trace("Аппроксимирующая линия имеет сильный убывающий тренд\n");
                    break;
                }
                case Trend.Up:
                {
                    logger.Trace("[Ширина коридора] = {0}\t", fi.flatWidth);
                    logger.Trace("[Минимальная ширина коридора] = {0}\n", _Constants.MinWidthCoeff * fi.Median);
                    logger.Trace("Аппроксимирующая линия имеет сильный возрастающий тренд\n");
                    break;
                }
                case Trend.Neutral:
                {
                    logger.Trace("[Ширина коридора] = {0}\t", fi.flatWidth);
                    logger.Trace("[Минимальная ширина коридора] = {0}\n", _Constants.MinWidthCoeff * fi.Median);
                    logger.Trace("Аппроксимирующая линия почти горизонтальна. Тренд нейтральный\n");
                    if (fi.IsFlat)
                    {
                        logger.Trace("Цена, вероятно, формирует боковик...\n");
                    }
                    break;
                }
                default: 
                    break;
            }

            if ((fi.flatWidth) < (_Constants.MinWidthCoeff * fi.Median))
            {
                logger.Trace("Боковик слишком узок\n");
            }
        }

        public void WhyIsNotFlat(_CandleStruct leftBound, _CandleStruct rightBound)
        {
            string reason = "";
            logger.Trace("Окно с {0} по {1}", leftBound.date, rightBound.date);
            logger.Trace("В окне не определено боковое движение.\nВозможные причины:");

            
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

            //Console.WriteLine(reason);
            logger.Trace(reason + "\n");
        }

        public void OutputHistoricalInfo()
        {
            logger.Trace("Боковиков найдено: {0}", hFf.FlatsFound);
            logger.Trace("Боковики определены в: ");
            for (int i = 0; i < hFf.ApertureBounds.Count; i++)
            {
                logger.Trace("[{0}]\t[{1}]", hFf.ApertureBounds[i].left.date, hFf.ApertureBounds[i].right.date);
            }
        }
    }
}

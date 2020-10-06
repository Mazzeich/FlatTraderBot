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
        
        private readonly FlatIdentifier flatIdentifier;
        private readonly HistoricalFlatFinder historicalFlatFinder;

        private Printer()
        {
            logger.Trace("[Printer] initialized");
        }
        public Printer(FlatIdentifier flatIdentifier) : this()
        {
            this.flatIdentifier = flatIdentifier;
        }

        public Printer(HistoricalFlatFinder historicalFf) : this()
        {
            historicalFlatFinder = historicalFf;
        }

        public void OutputApertureInfo()
        {
            logger.Trace("Окно {0} с {1} по {2}", 
                flatIdentifier.FlatBounds.left.date, 
                flatIdentifier.FlatBounds.left.time, 
                flatIdentifier.FlatBounds.right.time);

            logger.Trace("[gMin] = {0} [{1}]\t[gMax] = {2} [{3}]", flatIdentifier.gMin, flatIdentifier.idxGmin + 1, flatIdentifier.gMax, flatIdentifier.idxGmax + 1);
            logger.Trace("[k] = {0}", flatIdentifier.k);
            logger.Trace("[mean] = {0}", flatIdentifier.mean);
            logger.Trace("[candles.Count] = {0}", flatIdentifier.candles.Count);
            logger.Trace("[SDL] = {0}\t\t[SDH] = {1}", flatIdentifier.SDL, flatIdentifier.SDH);
            logger.Trace("[Экстремумы рядом с СКО low] = {0}\t[Экстремумы рядом с СКО high] = {1}", flatIdentifier.exsNearSDL, flatIdentifier.exsNearSDH);
            logger.Trace("[Границы окна]: [{0}]\t[{1}]", flatIdentifier.FlatBounds.left.date, flatIdentifier.FlatBounds.right.date);
            
            switch (flatIdentifier.trend)
            {
                case Trend.Down:
                {
                    logger.Trace("[Ширина коридора] = {0}\t", flatIdentifier.flatWidth);
                    logger.Trace("[Минимальная ширина коридора] = {0}", _Constants.MinWidthCoeff * flatIdentifier.mean);
                    logger.Trace("Аппроксимирующая линия имеет сильный убывающий тренд");
                    break;
                }
                case Trend.Up:
                {
                    logger.Trace("[Ширина коридора] = {0}\t", flatIdentifier.flatWidth);
                    logger.Trace("[Минимальная ширина коридора] = {0}", _Constants.MinWidthCoeff * flatIdentifier.mean);
                    logger.Trace("Аппроксимирующая линия имеет сильный возрастающий тренд\n");
                    break;
                }
                case Trend.Neutral:
                {
                    logger.Trace("[Ширина коридора] = {0}\t", flatIdentifier.flatWidth);
                    logger.Trace("[Минимальная ширина коридора] = {0}", _Constants.MinWidthCoeff * flatIdentifier.mean);
                    logger.Trace("Аппроксимирующая линия почти горизонтальна. Тренд нейтральный");
                    if (flatIdentifier.isFlat)
                    {
                        logger.Trace("Цена, вероятно, формирует боковик...\n");
                    }
                    break;
                }
                default: 
                    break;
            }

            if ((flatIdentifier.flatWidth) < (_Constants.MinWidthCoeff * flatIdentifier.mean))
            {
                logger.Trace("Боковик слишком узок\n");
            }
        }

        public void OutputHistoricalInfo()
        {
            if (historicalFlatFinder != null)
            {
                logger.Trace("Боковиков найдено: {0}", historicalFlatFinder.flatsFound);
                logger.Trace("Боковики определены в: ");
                for (int i = 0; i < historicalFlatFinder.apertureBounds.Count; i++)
                {
                    logger.Trace("[{0}] с [{1}] по [{2}]",
                        historicalFlatFinder.apertureBounds[i].left.date,
                        historicalFlatFinder.apertureBounds[i].left.time,
                        historicalFlatFinder.apertureBounds[i].right.time);
                }
            }
            else
            {
                logger.Debug("[Printer.OutputHistoricalInfo().historicalFlatFinder] == null");
            }
        }
    }
}

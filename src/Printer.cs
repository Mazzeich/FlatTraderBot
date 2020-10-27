using System;
using System.Collections.Generic;
using NLog;

namespace FlatTraderBot
{
    internal class Printer
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        private readonly FlatIdentifier flatIdentifier;
        private readonly HistoricalFlatFinder historicalFlatFinder;

        private Printer()
        {
        }
        public Printer(FlatIdentifier flatIdentifier) : this()
        {
            this.flatIdentifier = flatIdentifier;
        }

        public Printer(HistoricalFlatFinder historicalFf) : this()
        {
            historicalFlatFinder = historicalFf;
        }

        [Obsolete("Use FlatIdentifier.LogFlatProperties() instead")]
        public void OutputApertureInfo()
        {
            logger.Trace("Окно {0} с {1} по {2}", 
                flatIdentifier.flatBounds.left.date, 
                flatIdentifier.flatBounds.left.time, 
                flatIdentifier.flatBounds.right.time);

            logger.Trace("[gMin] = {0} [{1}]\t[gMax] = {2} [{3}]", flatIdentifier.gMin, flatIdentifier.idxGmin + 1, flatIdentifier.gMax, flatIdentifier.idxGmax + 1);
            logger.Trace("[k] = {0}", flatIdentifier.k);
            logger.Trace("[mean] = {0}", flatIdentifier.mean);
            logger.Trace("[candles.Count] = {0}", flatIdentifier.candles.Count);
            logger.Trace("[SDMean] = {0}\t[SDL] = {1}\t[SDH] = {2}", flatIdentifier.SDMean, flatIdentifier.SDL, flatIdentifier.SDH);
            logger.Trace("[Экстремумы рядом с СКО low] = {0}\t[Экстремумы рядом с СКО high] = {1}", flatIdentifier.exsNearSDL, flatIdentifier.exsNearSDH);
            logger.Trace("[Границы окна]: [{0}]\t[{1}]", flatIdentifier.flatBounds.left.date, flatIdentifier.flatBounds.right.date);
            
            switch (flatIdentifier.trend)
            {
                case Direction.Down:
                {
                    logger.Trace("[Ширина коридора] = {0}\t", flatIdentifier.flatWidth);
                    logger.Trace("[Минимальная ширина коридора] = {0}", _Constants.MinWidthCoeff * flatIdentifier.mean);
                    logger.Trace("Аппроксимирующая линия имеет сильный убывающий тренд");
                    break;
                }
                case Direction.Up:
                {
                    logger.Trace("[Ширина коридора] = {0}\t", flatIdentifier.flatWidth);
                    logger.Trace("[Минимальная ширина коридора] = {0}", _Constants.MinWidthCoeff * flatIdentifier.mean);
                    logger.Trace("Аппроксимирующая линия имеет сильный возрастающий тренд\n");
                    break;
                }
                case Direction.Neutral:
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
                foreach (FlatIdentifier flat in historicalFlatFinder.flatList)
                {
                    logger.Trace($"[{flat.flatBounds.left.date}] с [{flat.flatBounds.left.time}] по [{flat.flatBounds.right.time}]");
                }                            
            }
            else
            {
                logger.Trace("[Printer.OutputHistoricalInfo().historicalFlatFinder] == null");
            }
        }

        public void PrintFlatListInfo(IEnumerable<FlatIdentifier> flatList)
        {
            foreach (FlatIdentifier flat in flatList)
            {
                // TODO: Красивый logger.Trace() всех полей объекта
            }
        }

        public void PrintReasonsApertureIsNotFlat()
        {
            logger.Trace($"[{flatIdentifier.candles[0].date}]: [{flatIdentifier.candles[0].time} {flatIdentifier.candles[^1].time}]: {flatIdentifier.reasonsOfApertureHasNoFlat}");
        }
    }
}

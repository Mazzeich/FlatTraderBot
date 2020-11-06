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
        { }
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
            logger.Trace($"Окно {flatIdentifier.bounds.left.date} с {flatIdentifier.bounds.left.time} по {flatIdentifier.bounds.right.time}");
            logger.Trace($"[gMin] = {flatIdentifier.gMin} [{0}]\t[gMax] = {flatIdentifier.gMax} [{1}]",  flatIdentifier.idxGmin + 1, flatIdentifier.idxGmax + 1);
            logger.Trace($"[k] = {flatIdentifier.k}");
            logger.Trace($"[mean] = {flatIdentifier.mean}");
            logger.Trace($"[candles.Count] = {flatIdentifier.candles.Count}");
            logger.Trace($"[SDMean] = {flatIdentifier.SDMean}\t[SDL] = {flatIdentifier.SDL}\t[SDH] = {flatIdentifier.SDH}");
            logger.Trace($"[Экстремумы рядом с СКО low] = {flatIdentifier.exsNearSDL}\t[Экстремумы рядом с СКО high] = {flatIdentifier.exsNearSDH}");
            logger.Trace($"[Границы окна]: [{flatIdentifier.bounds.left.date}]\t[{flatIdentifier.bounds.right.date}]");
            
            switch (flatIdentifier.trend)
            {
                case Direction.Down:
                {
                    logger.Trace($"[Ширина коридора] = {flatIdentifier.width}\t");
                    logger.Trace("[Минимальная ширина коридора] = {0}", _Constants.MinWidthCoeff * flatIdentifier.mean);
                    logger.Trace("Аппроксимирующая линия имеет сильный убывающий тренд");
                    break;
                }
                case Direction.Up:
                {
                    logger.Trace($"[Ширина коридора] = {flatIdentifier.width}\t");
                    logger.Trace("[Минимальная ширина коридора] = {0}", _Constants.MinWidthCoeff * flatIdentifier.mean);
                    logger.Trace("Аппроксимирующая линия имеет сильный возрастающий тренд\n");
                    break;
                }
                case Direction.Neutral:
                {
                    logger.Trace($"[Ширина коридора] = {flatIdentifier.width}\t");
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

            if ((flatIdentifier.width) < (_Constants.MinWidthCoeff * flatIdentifier.mean))
            {
                logger.Trace("Боковик слишком узок\n");
            }
        }

        public void OutputHistoricalInfo()
        {
            if (historicalFlatFinder != null)
            {
                logger.Trace($"Боковиков найдено: {historicalFlatFinder.flatsFound}");
                logger.Trace("Боковики определены в: ");
                foreach (FlatIdentifier flat in historicalFlatFinder.flatList)
                {
                    logger.Trace($"[{flat.bounds.left.date}] с [{flat.bounds.left.time}] по [{flat.bounds.right.time}]");
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

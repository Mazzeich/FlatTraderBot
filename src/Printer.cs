using FlatTraderBot.Structs;
using NLog;
using System;
using System.Collections.Generic;

namespace FlatTraderBot
{
    internal class Printer
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        private readonly FlatIdentifier flat;
        private readonly HistoricalFlatFinder historicalFlatFinder;

        private Printer()
        { }
        public Printer(FlatIdentifier flat) : this()
        {
            this.flat = flat;
        }

        public Printer(HistoricalFlatFinder historicalFf) : this()
        {
            historicalFlatFinder = historicalFf;
        }

        [Obsolete("Use FlatIdentifier.LogFlatProperties() instead")]
        public void OutputApertureInfo()
        {
            logger.Trace($"Окно {flat.bounds.left.date} с {flat.bounds.left.time} по {flat.bounds.right.time}");
            logger.Trace($"[gMin] = {flat.gMin} [{0}]\t[gMax] = {flat.gMax} [{1}]",  flat.idxGmin + 1, flat.idxGmax + 1);
            logger.Trace($"[k] = {flat.k}");
            logger.Trace($"[mean] = {flat.mean}");
            logger.Trace($"[candles.Count] = {flat.candles.Count}");
            logger.Trace($"[SDMean] = {flat.SDMean}\t[SDL] = {flat.SDL}\t[SDH] = {flat.SDH}");
            logger.Trace($"[Экстремумы рядом с СКО low] = {flat.exsNearSDL}\t[Экстремумы рядом с СКО high] = {flat.exsNearSDH}");
            logger.Trace($"[Границы окна]: [{flat.bounds.left.date}]\t[{flat.bounds.right.date}]");
            
            switch (flat.trend)
            {
                case Direction.Down:
                {
                    logger.Trace($"[Ширина коридора] = {flat.width}\t");
                    logger.Trace("[Минимальная ширина коридора] = {0}", _Constants.MinWidthCoeff * flat.mean);
                    logger.Trace("Аппроксимирующая линия имеет сильный убывающий тренд");
                    break;
                }
                case Direction.Up:
                {
                    logger.Trace($"[Ширина коридора] = {flat.width}\t");
                    logger.Trace("[Минимальная ширина коридора] = {0}", _Constants.MinWidthCoeff * flat.mean);
                    logger.Trace("Аппроксимирующая линия имеет сильный возрастающий тренд\n");
                    break;
                }
                case Direction.Neutral:
                {
                    logger.Trace($"[Ширина коридора] = {flat.width}\t");
                    logger.Trace("[Минимальная ширина коридора] = {0}", _Constants.MinWidthCoeff * flat.mean);
                    logger.Trace("Аппроксимирующая линия почти горизонтальна. Тренд нейтральный");
                    if (flat.isFlat)
                    {
                        logger.Trace("Цена, вероятно, формирует боковик...\n");
                    }
                    break;
                }
                default: 
                    break;
            }

            if ((flat.width) < (_Constants.MinWidthCoeff * flat.mean))
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

        public void PrintReasonsApertureIsNotFlat()
        {
            logger.Trace($"[{flat.candles[0].date}]: [{flat.candles[0].time} {flat.candles[^1].time}]: {flat.reasonsOfApertureHasNoFlat}");
        }
    }
}

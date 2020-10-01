using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

// ReSharper disable CommentTypo

namespace Lua
{
    public class HistoricalFlatFinder
    {
        /// <summary>
        /// Инициализация логгера
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        // TODO: Коллекция окон, чтобы можно было итерироваться по каждому и выводить информацию адекватнее
        
        private readonly List<_CandleStruct> globalCandles;
        
        private List<_CandleStruct> aperture = new List<_CandleStruct>(_Constants.NAperture);

        /// <summary>
        /// Сколько боковиков было найдено
        /// </summary>
        public int FlatsFound { get; private set; }

        public List<Bounds> ApertureBounds { get; } = new List<Bounds>();

        private HistoricalFlatFinder()
        {
            logger.Trace("\n[HistoricalFlatFinder] initialized");
        }

        public HistoricalFlatFinder(List<_CandleStruct> candles) : this()
        {
            globalCandles = candles;

            for (int i = 0; i < _Constants.NAperture; i++) // Формируем стартовое окно
            {
                aperture.Add(globalCandles[i]);
            }

            FindAllFlats();
        }

        private void FindAllFlats()
        {
            for (int i = 0; i < globalCandles.Count;)
            {
                MoveAperture(i);

                int localAddedCandles = 0;
                if (globalCandles.Count - i < _Constants.NAperture - 1) break;

                FlatIdentifier flatIdentifier = new FlatIdentifier(aperture);
                flatIdentifier.Identify();
                // Если не определили боковик сходу
                if (flatIdentifier.isFlat == false)
                {
                    i++;
                    MoveAperture(i);
                    continue;
                }

                while (flatIdentifier.isFlat == true)
                {
                    flatIdentifier.Identify();
                    localAddedCandles++;
                    ExpandAperture(i);
                    if (flatIdentifier.isFlat == false)
                    {
                        FlatsFound++;
                        logger.Trace("Боковик определён в [{0}] с [{1}] по [{2}]", 
                            flatIdentifier.FlatBounds.left.date,
                            flatIdentifier.FlatBounds.left.time,
                            flatIdentifier.FlatBounds.right.time);
                    }
                }

                i += localAddedCandles;
            }
        }


        private void MoveAperture(int i)
        {
            //logger.Trace("[MoveAperture()]");
            aperture.Clear();

            try
            {
                for (int j = i; j < i + _Constants.NAperture; j++)
                {
                    aperture.Add(globalCandles[j]);
                }
            }
            catch (Exception exception)
            {
                logger.Trace(exception);
            }
        }


        private void ExpandAperture(int i)
        {
            aperture.Add(globalCandles[i + aperture.Count + 1]);
            //logger.Trace("Aperture expanded...\t[aperture.Count] = {0}", aperture.Count);
        }
    }
}
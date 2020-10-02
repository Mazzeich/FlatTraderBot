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
            for (int i = 0; i < globalCandles.Count;) // Как правило, i хранит в себе индекс начала окна во всё датасете
            {
                int localAddedCandles = 0;
                if (globalCandles.Count - i < _Constants.NAperture - 1)
                {
                    break;
                }

                FlatIdentifier flatIdentifier = new FlatIdentifier(aperture);
                flatIdentifier.Identify(); // Определяем начальное окно
                
                // Если не определили боковик сходу
                if (!flatIdentifier.isFlat)
                {
                    i++;
                    MoveAperture(i);
                    continue;
                }

                while (flatIdentifier.isFlat)
                {
                    flatIdentifier.Identify();
                    localAddedCandles++;
                    ExpandAperture(i);
                    if (!flatIdentifier.isFlat)
                    {
                        FlatsFound++;
                        logger.Trace("Боковик определён в [{0}] с [{1}] по [{2}]", 
                            flatIdentifier.FlatBounds.left.date,
                            flatIdentifier.FlatBounds.left.time,
                            flatIdentifier.FlatBounds.right.time);
                    }
                }

                i += localAddedCandles + aperture.Count; // Переместить i на следующую после найденного окна свечу
                MoveAperture(i - localAddedCandles); // Записать в окно новый лист с i-го по (i + _Constants.NAperture)-й в aperture
            }
        }
        
        /// <summary>
        /// Перемещает окно в следующую позицию (переинициализирует в следующем интервале)
        /// </summary>
        /// <param name="i">Начальный индекс, с которого будет начинать новое окно на + _Constants.NAperture</param>
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

        /// <summary>
        /// Расширяет окно на 1 свечу
        /// </summary>
        /// <param name="i">Начальный индекс, с которого расширять на + (aperture.Count + 1)</param>
        private void ExpandAperture(int i)
        {
            aperture.Add(globalCandles[i + aperture.Count + 1]);
            //logger.Trace("Aperture expanded...\t[aperture.Count] = {0}", aperture.Count);
        }
    }
}
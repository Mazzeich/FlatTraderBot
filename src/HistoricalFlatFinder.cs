using System;
using System.Collections.Generic;
using NLog;

// ReSharper disable CommentTypo

namespace FlatTraderBot
{
    public class HistoricalFlatFinder
    {
        /// <summary>
        /// Фабричный метод создания объектов
        /// </summary>
        /// <param name="candleStructs">Список свечей окна</param>
        /// <returns></returns>
        private static FlatIdentifier CreateInstance(List<_CandleStruct> candleStructs)
        {
            return new FlatIdentifier(candleStructs);
        }

        private HistoricalFlatFinder()
        {
            logger.Trace("\n[HistoricalFlatFinder] initialized");
        }

        public HistoricalFlatFinder(List<_CandleStruct> candles) : this()
        {
            globalCandles = candles;

            for (int i = 1; i < _Constants.NAperture; i++) // Формируем стартовое окно
            {
                _aperture.Add(globalCandles[i]);
            }
        }

        public void FindAllFlats()
        {
            // Как правило, globalIterator хранит в себе индекс начала окна во всём датасете
            for (int globalIterator = 0; globalIterator < globalCandles.Count - _Constants.NAperture - 1;) 
            {
                FlatIdentifier flatIdentifier = CreateInstance(_aperture);
                flatIdentifier.Identify(); // Определяем начальное окно
                
                // Если не определили боковик сходу
                if (!flatIdentifier.isFlat)
                {
                    Printer printer = new Printer(flatIdentifier);
                    printer.PrintReasonsApertureIsNotFlat();
                    flatIdentifier = null;
                    globalIterator++;
                    MoveAperture(ref globalIterator);
                    continue;
                }

                while (flatIdentifier.isFlat)
                {
                    // ExpansionRate раз...
                    for (int j = 0; j < _Constants.ExpansionRate; j++)
                    {
                        try
                        {
                            // ... расширяем окно на 1 свечу
                            ExtendAperture(globalIterator);
                        }
                        catch (Exception exception)
                        {
                            logger.Trace(exception);
                            return;
                        }
                    }
                    
                    flatIdentifier.Identify(); // Identify() вызывает SetBounds() сам

                    if (flatIdentifier.isFlat) 
                        continue; // Райдер предложил
                    
                    Printer printer = new Printer(flatIdentifier);
                    printer.PrintReasonsApertureIsNotFlat();
                    flatList.Add(flatIdentifier);
                    flatsFound++;
                    logger.Trace("Боковик определён в [{0}] с [{1}] по [{2}]", 
                        flatIdentifier.flatBounds.left.date, 
                        flatIdentifier.flatBounds.left.time,
                        flatIdentifier.flatBounds.right.time);

                    globalIterator += _aperture.Count; // Переместить i на следующую после найденного окна свечу

                    try
                    {
                        MoveAperture(ref globalIterator); // Записать в окно новый лист с i-го по (i + _Constants.NAperture)-й в aperture
                    }
                    catch (ArgumentOutOfRangeException exception)
                    {
                        logger.Warn("Argument out of range");
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Перемещает окно в следующую позицию (переинициализирует в следующем интервале)
        /// </summary>
        /// <param name="i">Начальный индекс, с которого будет начинать новое окно на + _Constants.NAperture</param>
        private void MoveAperture(ref int i)
        {
            logger.Trace("[MoveAperture()]");
            
            _aperture.Clear();
            // Если первая и последняя свечи будущего окна находятся в пределах одного дня
            if (globalCandles[i].date == globalCandles[i + _Constants.NAperture].date)
            {
                for (int j = i; j < i + _Constants.NAperture; j++)
                {
                    _aperture.Add(globalCandles[j]);
                }
            }
            else
            {
                logger.Trace("Начало и конец предполагаемого окна находятся в разных днях.");
                int indexOfTheNextDay = 0;
                // Находим начало следующего дня, где дата свечи не совпадает с датой свечи самого начала окна
                for (int j = i + 1; j < i + _Constants.NAperture; j++)
                {
                    indexOfTheNextDay = globalCandles[j].index;

                    if (globalCandles[j].date != globalCandles[i].date)
                    {
                        break;
                    }
                }

                i = indexOfTheNextDay + 1;

                for (int j = i; j < i + _Constants.NAperture; j++)
                {
                    _aperture.Add(globalCandles[j]);
                }
            }
            logger.Trace("[{0}] [{1}]", _aperture[0].time, _aperture[^1].time);
        }

        /// <summary>
        /// Расширяет окно на 1 свечу
        /// </summary>
        /// <param name="i">Начальный индекс, к которому добавить (aperture.Count + 1)</param>
        private void ExtendAperture(int i)
        {
            int indexOfAddingCandle = i + _aperture.Count + 1;
            _aperture.Add(globalCandles[indexOfAddingCandle]);
            logger.Trace("Aperture extended...\t[{0}][{1}]\t[aperture.Count] = {2}", _aperture[0].time, _aperture[^1].time, _aperture.Count);
        }

        /// <summary>
        /// Функция склеивает находящиеся близко друг к другу боковики
        /// </summary>
        private void UniteFlats()
        {
            logger.Trace("Uniting [flatList]...");
            for (int i = 1; i < flatsFound; i++)
            {
                FlatIdentifier currentFlat = flatList[i];
                FlatIdentifier prevFlat = flatList[i-1];
                
                if (currentFlat.flatBounds.left.date == prevFlat.flatBounds.left.date &&
                    currentFlat.flatBounds.left.index - prevFlat.flatBounds.right.index <= _Constants.MinFlatGap &&
                    Math.Abs(currentFlat.mean - prevFlat.mean) <= _Constants.flatsMeanOffset)
                {
                    
                }
            }
        }
        
        /// <summary>
        /// Инициализация логгера
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Основной, глобальный список свечей
        /// </summary>
        private readonly List<_CandleStruct> globalCandles;
        /// <summary>
        /// Маленький список свечей, формирующий окно
        /// </summary>
        private readonly List<_CandleStruct> _aperture = new List<_CandleStruct>(_Constants.NAperture);
        /// <summary>
        /// Сколько боковиков было найдено
        /// </summary>
        public int flatsFound { get; private set; }
        /// <summary>
        /// Маленький список свечей, формирующий окно
        /// </summary>
        private protected List<_CandleStruct> aperture => _aperture;
        /// <summary>
        /// Список всех найденных боковиков
        /// </summary>
        public readonly List<FlatIdentifier> flatList = new List<FlatIdentifier>();
    }
}
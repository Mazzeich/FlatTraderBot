using System;
using System.Collections.Generic;
using NLog;

// ReSharper disable CommentTypo

namespace FlatTraderBot
{
    public class HistoricalFlatFinder
    {
        /// <summary>
        /// Список всех найденных боковиков
        /// </summary>
        public readonly List<FlatIdentifier> flatList = new List<FlatIdentifier>();

        private HistoricalFlatFinder()
        {
            logger.Trace("\n[HistoricalFlatFinder] initialized");
        }

        public HistoricalFlatFinder(List<_CandleStruct> candles) : this()
        {
            globalCandles = candles;

            for (int i = 1; i < _Constants.NAperture; i++) // Формируем стартовое окно
            {
                aperture.Add(globalCandles[i]);
            }
        }

        public void FindAllFlats()
        {
            // Как правило, globalIterator хранит в себе индекс начала окна во всём датасете
            for (int globalIterator = 0; globalIterator < globalCandles.Count - _Constants.NAperture - 1;) 
            {
                FlatIdentifier flatIdentifier = new FlatIdentifier(ref aperture);
                flatIdentifier.Identify(); // Определяем начальное окно
                
                // Если не определили боковик сходу
                if (!flatIdentifier.isFlat)
                {
                    Printer printer = new Printer(flatIdentifier);
                    printer.PrintReasonsApertureIsNotFlat();
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
                    //flatsBounds.Add(flatIdentifier.flatBounds);
                    flatList.Add(flatIdentifier);
                    flatsFound++;
                    logger.Trace("Боковик определён в [{0}] с [{1}] по [{2}]", 
                        flatIdentifier.flatBounds.left.date, 
                        flatIdentifier.flatBounds.left.time,
                        flatIdentifier.flatBounds.right.time);
                    
                    
                    globalIterator += aperture.Count; // Переместить i на следующую после найденного окна свечу

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
            
            aperture.Clear();
            // Если первая и последняя свечи будущего окна находятся в пределах одного дня
            if (globalCandles[i].date == globalCandles[i + _Constants.NAperture].date)
            {
                for (int j = i; j < i + _Constants.NAperture; j++)
                {
                    aperture.Add(globalCandles[j]);
                }
            }
            else // иначе
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
                    aperture.Add(globalCandles[j]);
                }
            }
            logger.Trace("[{0}] [{1}]", aperture[0].time, aperture[^1].time);
        }

        /// <summary>
        /// Расширяет окно на 1 свечу
        /// </summary>
        /// <param name="i">Начальный индекс, к которому добавить (aperture.Count + 1)</param>
        private void ExtendAperture(int i)
        {
            int indexOfAddingCandle = i + aperture.Count + 1;
            aperture.Add(globalCandles[indexOfAddingCandle]);
            logger.Trace("Aperture extended...\t[{0}][{1}]\t[aperture.Count] = {2}", aperture[0].time, aperture[^1].time, aperture.Count);
        }

        /// <summary>
        /// Функция склеивает находящиеся близко друг к другу боковики
        /// </summary>
        /// <param name="_flats">Список границ всех найденных боковиков</param>
        private void UniteFlats(ref List<FlatIdentifier> _flats)
        {
            logger.Trace("Uniting flatList...");
            // TODO: Объединять боковики, близко расположенные друг к другу, и находящиеся на одном уровне
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
        private List<_CandleStruct> aperture = new List<_CandleStruct>(_Constants.NAperture);

        /// <summary>
        /// Сколько боковиков было найдено
        /// </summary>
        public int flatsFound { get; private set; }
    }
}
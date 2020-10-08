using System;
using System.Collections;
using System.Collections.Generic;
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
        
        /// <summary>
        /// Основной, глобальный список свечей
        /// </summary>
        private readonly List<_CandleStruct> globalCandles;
        
        private List<_CandleStruct> aperture = new List<_CandleStruct>(_Constants.NAperture);

        /// <summary>
        /// Сколько боковиков было найдено
        /// </summary>
        public int flatsFound { get; private set; }
        
        public List<Bounds> apertureBounds { get; } = new List<Bounds>();

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
            for (int globalIterator = 0; globalIterator < globalCandles.Count;) 
            {
                if (globalIterator + _Constants.NAperture > globalCandles.Count - 1)
                    return;

                FlatIdentifier flatIdentifier = new FlatIdentifier(ref aperture);
                flatIdentifier.Identify(); // Определяем начальное окно
                
                // Если не определили боковик сходу
                if (!flatIdentifier.isFlat)
                {
                    Printer printer = new Printer(flatIdentifier);
                    printer.ReasonsApertureIsNotFlat();
                    globalIterator++;
                    MoveAperture(globalIterator);
                    continue;
                }

                while (flatIdentifier.isFlat)
                {
                    //--------------------------------------------------
                    // TODO: Короче если мы нашли второй или больше бокович, проверяем,
                    // сколько свечей между его левым краем и правым краем предыдущего.
                    // Если мало, то сносим предыдущий бокович, взяв его левый край, и присобачив его позицию
                    // в левый край текущего окна. болдёш
                    //--------------------------------------------------
                    for (int j = 0; j < _Constants.ExpansionValue; j++)
                    {
                        try
                        {
                            ExpandAperture(globalIterator);
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
                    printer.ReasonsApertureIsNotFlat();
                    apertureBounds.Add(flatIdentifier.FlatBounds);
                    flatsFound++;
                    logger.Trace("Боковик определён в [{0}] с [{1}] по [{2}]", 
                        flatIdentifier.FlatBounds.left.date,
                        flatIdentifier.FlatBounds.left.time,
                        flatIdentifier.FlatBounds.right.time);
                    
                    
                    globalIterator += aperture.Count; // Переместить i на следующую после найденного окна свечу

                    try
                    {
                        MoveAperture(globalIterator); // Записать в окно новый лист с i-го по (i + _Constants.NAperture)-й в aperture
                    }
                    catch (Exception exception)
                    {
                        logger.Trace(exception);
                        return;
                    }
                }
            }
        }
        
        /// <summary>
        /// Перемещает окно в следующую позицию (переинициализирует в следующем интервале)
        /// </summary>
        /// <param name="i">Начальный индекс, с которого будет начинать новое окно на + _Constants.NAperture</param>
        private void MoveAperture(int i)
        {
            logger.Trace("[MoveAperture()]");
            
            aperture.Clear();
            for (int j = i; j < i + _Constants.NAperture; j++)
            {
                aperture.Add(globalCandles[j]);
            }
        }

        /// <summary>
        /// Расширяет окно на 1 свечу
        /// </summary>
        /// <param name="i">Начальный индекс, к которому добавить (aperture.Count + 1)</param>
        private void ExpandAperture(int i)
        {
            int indexOfAddingCandle = i + aperture.Count + 1;
            aperture.Add(globalCandles[indexOfAddingCandle]);
            logger.Trace("Aperture expanded...\t[aperture.Count] = {0}", aperture.Count);
        }

        /// <summary>
        /// Функция удаляет слишком близко расположенное окно и расширяет текущее до границ удалённого
        /// </summary>
        // public void UniteApertures()
        // {
        //     for (int i = 0; i < apertureBounds.Count - 1; i++)
        //     {
        //         if (apertureBounds[i].left.date == apertureBounds[i + 1].right.date)
        //         {
        //             if (apertureBounds[i + 1].left.time - apertureBounds[i].right.time <= 10)
        //             {
        //                 Bounds temp = apertureBounds[i];
        //                 temp.right.time = apertureBounds[i + 1].right.time;
        //                 apertureBounds[i] = temp;
        //                 
        //                 apertureBounds.RemoveAt(i + 1);
        //                 
        //                 flatsFound--;
        //                 logger.Trace("Flat removed");
        //             }
        //         }
        //     }
        // }
    }
}
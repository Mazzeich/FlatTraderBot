using FlatTraderBot.Structs;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlatTraderBot
{
    public class HistoricalFlatFinder
    {
        private HistoricalFlatFinder() {}

        public HistoricalFlatFinder(List<_CandleStruct> candles, ref List<FlatIdentifier> flatList) : this()
        {
            globalCandles = candles;
            this.flatList = flatList;

            for (int i = 1; i < _Constants.NAperture; i++) // Формируем стартовое окно
            {
                aperture.Add(globalCandles[i]);
            }
        }

        /// <summary> Основная функция, выполняющая поиск всех боковиков в глобальном списке свечей </summary>
        public void FindAllFlats()
        {
            // Как правило, globalIterator хранит в себе индекс начала окна во всём датасете

            for (int globalIterator = 0; globalIterator < globalCandles.Count - _Constants.NAperture * 2;)
            {
                FlatIdentifier flat = new FlatIdentifier();
                flat.AssignAperture(aperture);
                flat.Identify(); // Определяем начальное окно
                
                // Если не определили боковик сходу
                if (!flat.isFlat)
                {
                    Printer printer = new Printer(flat);
                    printer.PrintReasonsApertureIsNotFlat();
                    globalIterator++;
                    MoveAperture(ref globalIterator);
                    continue;
                }

                while (flat.isFlat)
                {
                    // ExpansionRate раз...
                    for (int j = 0; j < _Constants.ExpansionRate; j++)
                    {
                        // ЕСЛИ не конец данных
                        if (globalIterator + aperture.Count + 1 != globalCandles[globalCandles.Count - 1].index)
                        {
                            // ... расширяем окно на 1 свечу
                            ExtendAperture(globalIterator, ref aperture);
                        }
                        else
                        {
                            flatList.Add(flat);
                            flatsFound++;
                            return;
                        }
                    }
                    flat.AssignAperture(aperture);
                    flat.Identify(); // Identify() вызывает SetBounds(), если isFlat == true

                    if (flat.isFlat) 
                        continue; 
                    
                    Printer printer = new Printer(flat);
                    printer.PrintReasonsApertureIsNotFlat();
                    flatList.Add(flat);
                    flatsFound++;

                    globalIterator += aperture.Count; // Переместить итератор на следующую после найденного окна свечу
                    MoveAperture(ref globalIterator); 
                }
            }
        }

        /// <summary>
        /// Перемещает окно в следующую позицию (переинициализирует в следующем интервале) <br/>
        /// Записывает в окно новый лист с i-го по (i + _Constants.NAperture)-й в aperture
        /// </summary>
        /// <param name="i">Начальный индекс, с которого будет начинаться новое окно до (i + _Constants.NAperture)</param>
        private void MoveAperture(ref int i)
        {
            aperture.Clear();
            if (i + _Constants.NAperture >= globalCandles.Count)
                return;
            // Если первая и последняя свечи будущего окна находятся в пределах одного дня
            for (int j = i; j < i + _Constants.NAperture; j++)
            {
                aperture.Add(globalCandles[j]);
            }
        }

        /// <summary> Расширяет окно на 1 свечу </summary>
        /// <param name="i">Начальный индекс, к которому добавить (aperture.Count + 1)</param>
        /// <param name="_aperture">Окно</param>
        private void ExtendAperture(int i, ref List<_CandleStruct> _aperture)
        {
            int indexOfAddingCandle = i + _aperture.Count + 1;
            _aperture.Add(globalCandles[indexOfAddingCandle]);
        }

        public void LogAllFlats()
        {
            foreach (FlatIdentifier flat in flatList)
            {
                logger.Trace($"Флет определён в [{flat.bounds.left.date}] с [{flat.bounds.left.time}] по [{flat.bounds.right.time}]");
            }
            logger.Trace($"FlatsOverall: {flatsFound}");
        }

        /// <summary> Инициализация логгера </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        /// <summary> Основной, глобальный список свечей </summary>
        private readonly List<_CandleStruct> globalCandles;
        /// <summary> Маленький список свечей, формирующий окно </summary>
        private List<_CandleStruct> aperture = new List<_CandleStruct>(_Constants.NAperture);
        /// <summary> Сколько боковиков было найдено </summary>
        public int flatsFound { get; private set; }
        /// <summary> Список всех найденных боковиков </summary>
        public readonly List<FlatIdentifier> flatList;
    }
}
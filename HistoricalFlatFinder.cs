using System;
using System.Collections.Generic;
using System.Runtime;

// ReSharper disable CommentTypo

namespace Lua
{
    public class HistoricalFlatFinder
    {
        private List<_CandleStruct> globalCandles = new List<_CandleStruct>(_Constants.NGlobal);
        private List<_CandleStruct> aperture = new List<_CandleStruct>(_Constants.NAperture);
        private List<Bounds> apertureBounds = new List<Bounds>();

        /// <summary>
        /// Сколько боковиков было найдено
        /// </summary>
        private int flatsFound;


        public int FlatsFound
        {
            get => flatsFound;
            set => this.flatsFound = value;
        }

        public List<Bounds> ApertureBounds
        {
            get => apertureBounds;
            set => apertureBounds = value;
        }

        public HistoricalFlatFinder(List<_CandleStruct> _candles)
        {
            Console.WriteLine("[HistoricalFlatFinder()]");
            globalCandles = _candles;

            for (int i = 0; i < _Constants.NAperture; i++) // Формируем стартовое окно
            {
                aperture.Add(globalCandles[i]);
            }
            Console.WriteLine("Стартовое окно: globalCandles.Length = {0}\taperture.Count = {1}", globalCandles.Count, aperture.Count);

            FindAllFlats();
        }

        private void FindAllFlats()
        {
            Console.WriteLine("[FindAllFlats()]");

            int overallAdded = 0;
            int localAddedCandles = 0;
            int step = 0;

            for (int i = 0; i < globalCandles.Count - 1; i += (_Constants.NAperture * step) + overallAdded)
            {
                step++;
                localAddedCandles = 0;
                Console.WriteLine("[i] = {0}\t[aperture.Count] = {1}", i, aperture.Count);
                if (globalCandles.Count - i <= _Constants.NAperture) // Если в конце осталось меньше свечей, чем вмещает окно
                {
                    break;
                }


                FlatIdentifier flatIdentifier = new FlatIdentifier(aperture);
                flatIdentifier.isFlat = false;

                flatIdentifier.Identify();

                while (flatIdentifier.isFlat)
                {
                    localAddedCandles++;
                    aperture.Add(globalCandles[(aperture.Count * step) - 1 + localAddedCandles]);
                    Console.WriteLine("Aperture expanded...\t[aperture.Count] = {0}", aperture.Count);
                    flatIdentifier.Identify();
                    
                    if (flatIdentifier.isFlat == false)
                    {
                        // TODO: Обработка результата
                        flatsFound++;
                        overallAdded += localAddedCandles;

                        Console.WriteLine("+1 боковик!");
                        Console.WriteLine("[overallAdded] = {0}", overallAdded);
                        //Console.WriteLine("Боковик определён в [{0}] [{1}]", flatIdentifier._Boudns.start.date, flatIdentifier._Boudns.end.date);
                        Bounds bounds;
                        bounds.start = flatIdentifier.Bounds.start;
                        bounds.end = flatIdentifier.Bounds.end;
                        apertureBounds.Add(bounds);
                    }
                }

                aperture = MoveAperture(overallAdded, step);
            }
        }

        /// <summary>
        /// Функция перемещения окна в следующую позцию
        /// </summary>
        /// <param name="_candlesToAdd">Всего свечей, которые были добавлены ранее</param>
        /// <param name="_step">Текущий шаг прохода алгоритма</param>
        /// <returns>Новое окно свечей</returns>
        private List<_CandleStruct> MoveAperture(int _candlesToAdd, int _step)
        {
            Console.WriteLine("[MoveAperture()]");
            aperture.Clear();

            
            int startPosition = (_Constants.NAperture * _step) + _candlesToAdd + 1;
            for (int i = startPosition; i < startPosition + _Constants.NAperture; i++)
            {
                aperture.Add(globalCandles[i]);
            }


            return aperture;
        }
    }
}
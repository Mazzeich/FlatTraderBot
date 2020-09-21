using System;
using System.Collections.Generic;

namespace Lua
{
    public class HistoricalFlatFinder
    {
        private List<_CandleStruct> globalCandles = new List<_CandleStruct>(_Constants.nGlobal);
        private List<_CandleStruct> aperture = new List<_CandleStruct>(_Constants.nAperture);


        public HistoricalFlatFinder(List<_CandleStruct> _candles)
        {
            Console.WriteLine("[HistoricalFlatFinder()]");
            globalCandles = _candles;

            for (int i = 0; i < _Constants.nAperture; i++) // Формируем стартовое окно
            {
                aperture.Add(globalCandles[i]);
            }
            Console.WriteLine("globalCandles.Length = {0}\taperture.Count = {1}", globalCandles.Count, aperture.Count);

            FindAllFlats();
        }

        private void FindAllFlats()
        {
            Console.WriteLine("[FindAllFlats()]");

            int overallAdded = 0;
            int addedCandles = 0;
            int step = 0;

            int flats = 0;

            for (int i = 0; i < globalCandles.Count - 1; i += (_Constants.nAperture * step) + overallAdded) // От 0 до 18977
            {
                FlatIdentifier flatIdentifier = new FlatIdentifier(aperture);

                addedCandles = 0;
                Console.WriteLine("[i] = {0}\t[aperture.Count] = {1}", i, aperture.Count);
                if (globalCandles.Count - i <= _Constants.nAperture - 1) // Если в конце осталось меньше свечей, чем вмещает окно
                {
                    break;
                }

                flatIdentifier.Identify();

                while (flatIdentifier.isFlat)
                {
                    addedCandles++;
                    aperture.Add(globalCandles[i + addedCandles]);
                    Console.WriteLine(aperture.Count + " " + (i + addedCandles));
                    //flatIdentifier.Expand(aperture[aperture.Count - 1]);
                    flatIdentifier.Identify();
                    if (flatIdentifier.isFlat == false)
                    {
                        // TODO: Обработка результата
                        Console.WriteLine("+1 боковик!");
                        Console.WriteLine(globalCandles[i].date);
                        flats++;
                    }
                }


                step++;
                overallAdded += addedCandles;
                flatIdentifier.isFlat = false;
                Console.WriteLine(flatIdentifier.isFlat + " " + overallAdded);
            }

            Console.WriteLine("Найдено боковиков = {0}", flats);
        }

        /// <summary>
        /// Функция перемещения окна в следующую позцию
        /// </summary>
        /// <param name="_candlesToAdd">Всего свечей, которые были добавлены ранее</param>
        /// <param name="_step">Текущий шаг прохода алгоритма</param>
        /// <returns>Новое окно свечей</returns>
        private List<_CandleStruct> MoveAperture(int _candlesToAdd, int _step)
        {
            aperture.Clear();
            for (int i = (_Constants.nAperture * _step) + _candlesToAdd; i < _Constants.nGlobal; i++)
            {
                aperture.Add(globalCandles[i]);
            }

            return aperture;
        }
    }
}
using System;
using System.Collections.Generic;

namespace Lua
{
    public class HistoricalFlatFinder
    {
        private _CandleStruct[] globalCandles;
        private List<_CandleStruct> aperture = new List<_CandleStruct>();


        public HistoricalFlatFinder(_CandleStruct[] _candles)
        {
            Console.WriteLine("[HistoricalFlatFinder()]");
            globalCandles = _candles;

            for (int i = 0; i < _Constants.nAperture; i++) // Формируем стартовое окно
            {
                aperture.Add(globalCandles[i]);
            }
            Console.WriteLine("globalCandles.Length = {0}\taperture.Count = {1}", globalCandles.Length, aperture.Count);

            FindAllFlats();
        }

        private void FindAllFlats()
        {
            Console.WriteLine("[FindAllFlats()]");
            FlatIdentifier flatIdentifier = new FlatIdentifier(aperture);
            for (int i = 0; i < globalCandles.Length - 1; i += aperture.Count) // От 0 до 18977
            {
                Console.WriteLine("for loop({0})", i);
                int candlesToAdd = 0;
                if (globalCandles.Length - i <= _Constants.nAperture - 1) // Если в конце осталось меньше свечей, чем вмещает окно
                {
                    break;
                }
                flatIdentifier.Identify();

                while (flatIdentifier.isFlat)
                {
                    Console.WriteLine(flatIdentifier.isFlat);
                    candlesToAdd++;
                    aperture.Add(globalCandles[]);
                    flatIdentifier.Expand(aperture[aperture.Count]);
                    flatIdentifier.Identify();
                }

            }
        }
    }
}
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
            globalCandles = _candles;

            for (int i = 0; i < _Constants.nAperture; i++) // Формируем стартовое окно
            {
                aperture.Add(globalCandles[i]);
            }

            FindAllFlats();
        }

        private void FindAllFlats()
        {
            FlatIdentifier flatIdentifier = new FlatIdentifier(aperture);
            
            for (int i = 0; i < globalCandles.Length - 1; i ++) // От 0 до 18977
            {
                if (globalCandles.Length - i <= _Constants.nAperture - 1) // Если в конце осталось меньше свечей, чем вмещает окно
                {
                    break;
                }


            }
        }
    }
}
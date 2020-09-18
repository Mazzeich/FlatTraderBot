using System;
using System.IO;
using System.Globalization;

namespace Lua
{    class Program
    {
        static void Main(string[] args)
        {
            _CandleStruct[] candles = new _CandleStruct[_Constants.nGlobal];

            Reader reader = new Reader(candles);
           
            candles = reader.GetHistoricalData();
            HistoricalFlatFinder historicalFlatFinder = new HistoricalFlatFinder(candles);

            return;
        }
    }
}


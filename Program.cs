using System;
using System.Collections.Generic;

namespace Lua
{
    internal static class Program
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        static void Main()
        {
            logger.Info("Program has started...");
            List<_CandleStruct> candles = new List<_CandleStruct>();
            Reader reader = new Reader(candles);
            candles = reader.GetHistoricalData();
            HistoricalFlatFinder historicalFlatFinder = new HistoricalFlatFinder(candles);
            Printer printer = new Printer(historicalFlatFinder);
            printer.OutputHistoricalInfo();

            logger.Info("Main() completed successfully.");
            Console.ReadKey();
        }
    }
}


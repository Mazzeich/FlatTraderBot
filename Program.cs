using System;
using System.Collections.Generic;
using NLog;
// ReSharper disable CommentTypo

namespace Lua
{
    internal static class Program
    {
        /// <summary>
        /// Инициализация логгера
        /// В документации указано, что это делают в каждом классе программы
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        static void Main()
        {
            Logger.Info("----------------------------------------------------------------------------------");
            Logger.Info("Program has started...");
            
            List<_CandleStruct> candles = new List<_CandleStruct>();
            Reader reader = new Reader(candles);
            candles = reader.GetHistoricalData();
            HistoricalFlatFinder historicalFlatFinder = new HistoricalFlatFinder(candles);
            Printer printer = new Printer(historicalFlatFinder);
            printer.OutputHistoricalInfo();

            Logger.Info("Main() completed successfully.");
            Console.ReadKey();
        }
    }
}


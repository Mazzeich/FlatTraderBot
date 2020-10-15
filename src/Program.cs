using System.Collections.Generic;
using NLog;

// ReSharper disable CommentTypo

namespace FlatTraderBot
{
    internal static class Program
    {
        /// <summary>
        /// Инициализация логгера
        /// В документации указано, что это делают в каждом классе программы
        /// </summary>
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static void Main()
        {
            logger.Trace("\nProgram has started...");
            
            List<_CandleStruct> candles = new List<_CandleStruct>();
            Reader reader = new Reader(candles);
            candles = reader.GetHistoricalData();
            
            HistoricalFlatFinder historicalFlatFinder = new HistoricalFlatFinder(candles);
            historicalFlatFinder.FindAllFlats();
            
            Printer printer = new Printer(historicalFlatFinder);
            printer.OutputHistoricalInfo();

            FlatClassifier flatClassifier = new FlatClassifier(historicalFlatFinder.flatList, candles);
            flatClassifier.ClassifyAllFlats();
            
            

            logger.Trace("Main() completed successfully.");
            LogManager.Shutdown();
        }
    }
}


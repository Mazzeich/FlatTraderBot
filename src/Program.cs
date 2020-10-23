using System.Collections.Generic;
using NLog;

namespace FlatTraderBot
{
    internal static class Program
    {
        private static void Main()
        {
            Logger logger = LogManager.GetCurrentClassLogger();
            logger.Trace("Program has started...");
            
            List<_CandleStruct> candles = new List<_CandleStruct>();
            candles = new Reader(candles).GetHistoricalData("data.csv");
            
            HistoricalFlatFinder historicalFlatFinder = new HistoricalFlatFinder(candles);
            historicalFlatFinder.FindAllFlats();
            
            FlatPostprocessor flatPostprocessor = new FlatPostprocessor(historicalFlatFinder);
            flatPostprocessor.UniteFlats();

            Printer printer = new Printer(historicalFlatFinder);
            printer.OutputHistoricalInfo();

            FlatClassifier flatClassifier = new FlatClassifier(historicalFlatFinder.flatList, candles);
            flatClassifier.ClassifyAllFlats();

            logger.Trace("Main() completed successfully.");
            LogManager.Shutdown();
        }
    }
}


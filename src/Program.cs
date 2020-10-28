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
            
            List<_CandleStruct>  candles  = new List<_CandleStruct>();
            List<FlatIdentifier> flatList = new List<FlatIdentifier>();
            
            candles = new Reader(candles).GetHistoricalData("data.csv");
            
            HistoricalFlatFinder historicalFlatFinder = new HistoricalFlatFinder(candles, ref flatList);
            historicalFlatFinder.FindAllFlats();
            
            FlatPostprocessor flatPostprocessor = new FlatPostprocessor(candles, ref flatList);
            flatPostprocessor.UniteFlats();

            FlatClassifier flatClassifier = new FlatClassifier(candles, ref flatList);
            flatClassifier.ClassifyAllFlats();
            flatClassifier.FindBreakthroughs();

            flatPostprocessor.UniteBreakthroughs();

            logger.Trace("Main() completed successfully.");
            LogManager.Shutdown();
        }
    }
}


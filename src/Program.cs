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

            FlatClassifier flatClassifier = new FlatClassifier(historicalFlatFinder.flatList, candles);
            flatClassifier.ClassifyAllFlats();
            flatClassifier.FindBreakthroughs();
            
            FlatPostprocessor flatPostprocessor = new FlatPostprocessor(historicalFlatFinder);
            flatPostprocessor.UniteFlats();
            
            flatClassifier = new FlatClassifier(flatPostprocessor.flatList, candles);
            flatClassifier.ClassifyAllFlats();
            flatClassifier.FindBreakthroughs();

            flatPostprocessor = new FlatPostprocessor(flatPostprocessor.flatList);
            flatPostprocessor.UniteBreakthroughs();

            logger.Trace("Main() completed successfully.");
            LogManager.Shutdown();
        }
    }
}


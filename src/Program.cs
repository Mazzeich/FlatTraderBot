using System.Collections.Generic;
using FlatTraderBot.Structs;
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
            
            candles = new Reader(candles).GetHistoricalData("data2020.csv");
            
            HistoricalFlatFinder historicalFlatFinder = new HistoricalFlatFinder(candles, ref flatList);
            historicalFlatFinder.FindAllFlats();
            
            FlatPostprocessor flatPostprocessor = new FlatPostprocessor(candles, ref flatList);
            flatPostprocessor.UniteFlats();
            logger.Trace($"Флетов после объединения: {flatList.Count}");

            FlatClassifier flatClassifier = new FlatClassifier(candles, ref flatList);
            flatClassifier.ClassifyAllFlats();
            
            StopLossesFinder stopLossesFinder = new StopLossesFinder(candles, ref flatList);
            stopLossesFinder.FindAndSetStopLosses();
            
            TakeProfitsFinder takeProfitsFinder = new TakeProfitsFinder(candles, ref flatList);
            takeProfitsFinder.FindAndSetTakeProfits();

            Dealer dealer = new Dealer(candles, flatList);
            dealer.SimulateDealing();
            
            logger.Trace("Main() completed successfully.");
            LogManager.Shutdown();
        }
    }
}


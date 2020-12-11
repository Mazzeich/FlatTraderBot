using FlatTraderBot.Structs;
using NLog;
using System.Collections.Generic;

namespace FlatTraderBot
{
    internal static class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static void Main() 
        {
            logger.Trace("Program has started...");
            string[] fileNames = 
                
            {
                "data.csv"        , "data2019.csv"    , "data2020.csv"     , "2half2020.csv"    , "1half2020.csv", 
                "5minData2020.csv", "5minData2019.csv", "5min2half2020.csv", "5min1half2020.csv", 
                "15minData2020.csv", "15minData20172020.csv", "1hourData20142020.csv"
            };

            // PermutateCoefficients(fileNames);
            CalculateLabel(fileNames[2]);

            logger.Trace("Main() completed successfully.");
            LogManager.Shutdown();
        }

        private static double CalculateLabel(string fileName)
        {
            List<_CandleStruct>  candles  = new List<_CandleStruct>();
            List<FlatIdentifier> flatList = new List<FlatIdentifier>();
            
            candles = new Reader(candles).GetHistoricalData(fileName);
            
            HistoricalFlatFinder historicalFlatFinder = new HistoricalFlatFinder(candles, ref flatList);
            historicalFlatFinder.FindAllFlats();
            if (flatList.Count <= _Constants.MinFlatsMustBeFound)
            {
                logger.Trace($"Not enough flats found ({flatList.Count})");
                return _Constants.InitialBalance;
            }

            historicalFlatFinder.LogAllFlats();
            
            FlatClassifier flatClassifier = new FlatClassifier(candles, ref flatList);
            flatClassifier.ClassifyAllFlats();
            
            StopLossesFinder stopLossesFinder = new StopLossesFinder(candles, ref flatList);
            stopLossesFinder.FindAndSetStopLosses();
            
            TakeProfitsFinder takeProfitsFinder = new TakeProfitsFinder(candles, ref flatList);
            takeProfitsFinder.FindAndSetTakeProfits();
            takeProfitsFinder.LogStatistics();
            
            Dealer dealer = new Dealer(candles, flatList);
            dealer.SimulateDealing();

            return dealer.balanceAccount;
        }
    }
}


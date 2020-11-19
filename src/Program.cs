using FlatTraderBot.Structs;
using NLog;
using System;
using System.Collections.Generic;

namespace FlatTraderBot
{
    internal static class Program
    {
        private static Logger logger;
        private static void Main()
        {
            logger = LogManager.GetCurrentClassLogger();
            logger.Trace("Program has started...");
            string[] fileNames = {"data.csv", "data2019.csv", "data2020.csv", "2half2020.csv", "1half2020.csv", 
                "5minData2020.csv", "5minData2019.csv", "5min2half2020.csv", "5min1half2020.csv"};

            // PermutateCoefficients(fileNames);
            // CalculateLabel(fileNames[5]);

            logger.Trace("Main() completed successfully.");
            LogManager.Shutdown();
        }

        private static double CalculateLabel(string fileName)
        {
            List<_CandleStruct>  candles  = new List<_CandleStruct>();
            List<FlatIdentifier> flatList = new List<FlatIdentifier>();
            
            candles = new Reader(candles).GetHistoricalData(fileNames[0]);
            
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

            logger.Trace("Main() completed successfully.");
            LogManager.Shutdown();
        }
    }
}


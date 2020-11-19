using FlatTraderBot.Structs;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;

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

        private static void PermutateCoefficients(string[] fileNames)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "dataset.csv");
            FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter writer  = new StreamWriter(fileStream);
            const string header = "ExpansionRate,NAperture,MinExtremums,MinWidthCoeff,KOffset,SDOffset,LeavingCoeff,Profit";
            NumberFormatInfo nfi = new CultureInfo( "en-US", false ).NumberFormat;
            nfi.NumberDecimalSeparator = ".";
            writer.WriteLine(header);
            _Constants.ExpansionRate = 3;
            _Constants.NAperture = 30; 
            _Constants.MinExtremumsNearSD = 3;

            _Constants.MinWidthCoeff = 0.0005;
            _Constants.KOffset = 0.0005;
            _Constants.SDOffset = 0.00025;
            _Constants.LeavingCoeff = 0.001;

            int i = 0;

            while (_Constants.LeavingCoeff <= 0.005)
            {
                _Constants.ExpansionRate++;
                if (_Constants.ExpansionRate >= 8)
                {
                    _Constants.ExpansionRate = 3;
                    _Constants.NAperture += 10;
                }

                if (_Constants.NAperture >= 60)
                {
                    _Constants.NAperture = 30;
                    _Constants.MinExtremumsNearSD++;
                }

                if (_Constants.MinExtremumsNearSD >= 4)
                {
                    _Constants.MinExtremumsNearSD = 3;
                    _Constants.MinWidthCoeff += 0.0005;
                }

                if (_Constants.MinWidthCoeff >= 0.0015)
                {
                    _Constants.MinWidthCoeff = 0.0005;
                    _Constants.KOffset += 0.0005;
                }

                if (_Constants.KOffset >= 0.005)
                {
                    _Constants.KOffset = 0.0005;
                    _Constants.SDOffset += 0.00005;
                }

                if (_Constants.SDOffset >= 0.00075)
                {
                    _Constants.SDOffset = 0.00025;
                    _Constants.LeavingCoeff += 0.001;
                }

                i++;

                double profit = CalculateLabel(fileNames[0]);
                writer.Write(_Constants.ExpansionRate + "," + _Constants.NAperture + "," +_Constants.MinExtremumsNearSD + ",", CultureInfo.CurrentCulture);
                writer.Write(_Constants.KOffset.ToString(nfi) + "," + _Constants.SDOffset.ToString(nfi) + "," + _Constants.LeavingCoeff.ToString(nfi) + "," + profit.ToString(nfi) + "\n");
                logger.Trace($"[{profit}]: ints {_Constants.ExpansionRate} {_Constants.NAperture} {_Constants.MinExtremumsNearSD} " +
                             $"doubles {_Constants.MinWidthCoeff} {_Constants.KOffset} {_Constants.SDOffset} {_Constants.LeavingCoeff}");
                logger.Trace(100 * i / 54000 + "%");
            }
            writer.Close();
        }
    }
}


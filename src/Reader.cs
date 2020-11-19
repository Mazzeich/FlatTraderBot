using CsvHelper;
using FlatTraderBot.Structs;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.ML;

namespace FlatTraderBot
{
    public class Reader
    {
        public Reader()
        {
            logger.Trace("\n[Class Reader initialized]");
        }
        
        public Reader(List<_CandleStruct> candleStruct) : this()
        {
            this.candleStruct = candleStruct;
            currentDirectory = Directory.GetCurrentDirectory();
        }

        ~Reader()
        {
            GC.SuppressFinalize(this);
        }

        /// <summary> Считывание свечей из csv-файла </summary>
        /// <param name="fileName">Имя файла с расширением</param>
        /// <returns>Список свечей</returns>
        public List<_CandleStruct> GetHistoricalData(string fileName)
        {
            logger.Trace("[GetHistoricalData invoked]");
            pathHistoricalData = Path.Combine(currentDirectory, @"Data\" + fileName);

            using StreamReader streamReader = new StreamReader(pathHistoricalData);
            using CsvReader csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
            logger.Trace($"Reading data from \"{pathHistoricalData}\"...");
            // TODO: GetFullPath

            csvReader.Read();
            csvReader.ReadHeader();

            int i = 0;
            int intraday = 0;
            while (csvReader.Read())
            {
                _CandleStruct temp;

                temp.index = i;
                temp.low   = csvReader.GetField<double>("<LOW>");
                temp.high  = csvReader.GetField<double>("<HIGH>");
                temp.open  = csvReader.GetField<double>("<OPEN>");
                temp.close = csvReader.GetField<double>("<CLOSE>");
                temp.date  = csvReader.GetField<string>("<DATE>");
                temp.time  = csvReader.GetField<string>("<TIME>");
                temp.avg   = (temp.high + temp.low) * 0.5;
                
                if (temp.time == "10:00")
                    intraday = 0;

                temp.intradayIndex = intraday;

                candleStruct.Add(temp);
                i++;
                intraday++;
            }

            logger.Trace($"Finished reading data from {pathHistoricalData}");
            return candleStruct;
        }

        public (IDataView, IDataView) GetMLData(string fileName)
        {
            pathHistoricalData = Path.Combine(currentDirectory, @"Data\" + fileName);

            MLContext context = new MLContext(seed: 0);
            IDataView data = context.Data.LoadFromTextFile<_CandleStruct>(pathHistoricalData, hasHeader: true, separatorChar: ',');
            
            // Split the data into a training set and a test set
            var trainTestData = context.Data.TrainTestSplit(data, testFraction: 0.2, seed: 0);
            IDataView trainData = trainTestData.TrainSet;
            IDataView testData = trainTestData.TestSet;

            return (trainData, testData);
        }
        
        private  static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly List<_CandleStruct> candleStruct;
        private static string currentDirectory;
        private string pathHistoricalData;
    }
}
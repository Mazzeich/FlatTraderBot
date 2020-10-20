using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using NLog;

// ReSharper disable CommentTypo

namespace FlatTraderBot
{
    public class Reader
    {
        /// <summary>
        /// Инициализация логгера и присваивание ему имени текущего класса "Lua.Reader"
        /// </summary>
        private  static readonly Logger logger = LogManager.GetCurrentClassLogger();
        List<_CandleStruct> candleStruct;

        private static string currentDirectory;
        
        private string pathHigh;
        private string pathLow;
        private string pathAvg;
        private string pathOpen;
        private string pathClose;
        private string pathVolume;
        private string pathHistoricalData;


        private string[] readHeights;
        private string[] readLows;
        private string[] readAvgs;
        private string[] readCloses;
        private string[] readOpens;
        private string[] readVolumes;

        public Reader(List<_CandleStruct> candleStruct)
        {
            logger.Trace("\n[Class Reader initialized]");
            this.candleStruct = candleStruct;
            currentDirectory = Directory.GetCurrentDirectory();
        }

        ~Reader()
        {
            logger.Trace("[Class Reader destroyed]");
        }

        /// <summary>
        /// Считать все строки из файлов распарсенных данных
        /// </summary>
        [Obsolete("Method were used to work with dataParser.lua")]
        public List<_CandleStruct> GetSeparatedData()
        {
            logger.Trace("[GetSeparatedData()] invoked. Reading data...");
            pathHigh   = Path.Combine(currentDirectory, @"..\..\..\Data\dataHigh.txt");
            pathLow    = Path.Combine(currentDirectory, @"..\..\..\Data\dataLow.txt");
            pathAvg    = Path.Combine(currentDirectory, @"..\..\..\Data\dataAvg.txt");
            pathOpen   = Path.Combine(currentDirectory, @"..\..\..\Data\dataOpen.txt");
            pathClose  = Path.Combine(currentDirectory, @"..\..\..\Data\dataClose.txt");
            pathVolume = Path.Combine(currentDirectory, @"..\..\..\Data\dataVolume.txt");

            readHeights = File.ReadAllLines(pathHigh);
            readLows    = File.ReadAllLines(pathLow);
            readAvgs    = File.ReadAllLines(pathAvg);
            readOpens   = File.ReadAllLines(pathOpen);
            readCloses  = File.ReadAllLines(pathClose);
            readVolumes = File.ReadAllLines(pathVolume);
        
            for (int i = 0; i < readHeights.Length; i++) //readHeights.Length = readLows.Length
            {
                _CandleStruct temp;
                temp.index = i;
                temp.low   = double.Parse(readLows[i]   , CultureInfo.InvariantCulture);
                temp.high  = double.Parse(readHeights[i], CultureInfo.InvariantCulture);
                temp.close = double.Parse(readCloses[i] , CultureInfo.InvariantCulture);
                temp.avg   = double.Parse(readAvgs[i]   , CultureInfo.InvariantCulture);
                temp.date  = "";
                temp.time  = "";

                candleStruct.Add(temp);
            }

            logger.Trace("[GetSeparatedData() finished]");
            return candleStruct;
        }

        public List<_CandleStruct> GetHistoricalData()
        {
            logger.Trace("[GetHistoricalData invoked]");
            pathHistoricalData = Path.Combine(currentDirectory, @"Data/data.csv");

            using StreamReader streamReader = new StreamReader(pathHistoricalData);
            using CsvReader csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
            logger.Trace("Reading data from \"{0}\"...", pathHistoricalData);
            // TODO: GetFullPath

            csvReader.Read();
            csvReader.ReadHeader();

            int i = 0;
            while (csvReader.Read())
            {
                _CandleStruct temp;

                temp.index = i;
                temp.low   = csvReader.GetField<double>("<LOW>");
                temp.high  = csvReader.GetField<double>("<HIGH>");
                temp.close = csvReader.GetField<double>("<CLOSE>");
                temp.avg   = (temp.high + temp.low) * 0.5;
                temp.date  = csvReader.GetField<string>("<DATE>");
                temp.time  = csvReader.GetField<string>("<TIME>");

                candleStruct.Add(temp);
                i++;
            }

            logger.Trace("Finished reading data from {0}", pathHistoricalData);
            return candleStruct;
        }
    }
}
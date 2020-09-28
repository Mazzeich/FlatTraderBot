using System.IO;
using System.Globalization;
using CsvHelper;
using System.Collections.Generic;

namespace Lua
{
    public class Reader
    {
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
        private string[] readAllData;

        public Reader(List<_CandleStruct> candleStruct)
        {
            this.candleStruct = candleStruct;
            currentDirectory = Directory.GetCurrentDirectory();
        }

        /// <summary>
        /// Считать все строки из файлов распарсенных данных
        /// </summary>
        public List<_CandleStruct> GetSeparatedData()
        {
            // Deprecated method
            pathHigh   = Path.Combine(currentDirectory, @"..\..\..\Data\dataHigh.txt");
            pathLow    = Path.Combine(currentDirectory, @"..\..\..\Data\dataLow.txt");
            pathAvg    = Path.Combine(currentDirectory, @"..\..\..\Data\dataAvg.txt");
            pathOpen   = Path.Combine(currentDirectory, @"..\..\..\Data\dataOpen.txt");
            pathClose  = Path.Combine(currentDirectory, @"..\..\..\Data\dataClose.txt");
            pathVolume = Path.Combine(currentDirectory, @"..\..\..\Data\dataVolume.txt");

            readHeights  = File.ReadAllLines(pathHigh);
            readLows     = File.ReadAllLines(pathLow);
            readAvgs     = File.ReadAllLines(pathAvg);
            readOpens    = File.ReadAllLines(pathOpen);
            readCloses   = File.ReadAllLines(pathClose);
            readVolumes  = File.ReadAllLines(pathVolume);
        
            for (int i = 0; i < readHeights.Length; i++) //readHeights.Length = readLows.Length
            {
                _CandleStruct temp;
                temp.low   = double.Parse(readLows[i], CultureInfo.InvariantCulture);
                temp.high  = double.Parse(readHeights[i], CultureInfo.InvariantCulture);
                temp.close = double.Parse(readCloses[i], CultureInfo.InvariantCulture);
                temp.avg   = double.Parse(readAvgs[i], CultureInfo.InvariantCulture);
                temp.date  = "";

                candleStruct.Add(temp);
            }

            return candleStruct;
        }

        public List<_CandleStruct> GetHistoricalData()
        {
            pathHistoricalData = Path.Combine(currentDirectory, @"..\..\..\Data\dataRTS.csv");

            using StreamReader streamReader = new StreamReader(pathHistoricalData);
            using CsvReader csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
            csvReader.Read();
            csvReader.ReadHeader();

            while (csvReader.Read())
            {
                _CandleStruct temp;
                
                temp.low = csvReader.GetField<double>("<LOW>");
                temp.high = csvReader.GetField<double>("<HIGH>");
                temp.close = csvReader.GetField<double>("<CLOSE>");
                temp.avg = (temp.high + temp.low) * 0.5;
                temp.date = csvReader.GetField<string>("<DATE>") + csvReader.GetField<string>("<TIME>");

                candleStruct.Add(temp);
            }

            return candleStruct;
        }
    }
}
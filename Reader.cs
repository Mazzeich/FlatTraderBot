using System;
using System.IO;
using System.Globalization;

namespace Lua
{
    public class Reader
    {
        _CandleStruct[] candleStruct;

        private static string currentDirectory;
        private string pathHigh;
        private string pathLow;
        private string pathAvg;
        private string pathOpen;
        private string pathClose;
        private string pathVolume;


        public string[] readHeights;
        public string[] readLows;
        public string[] readAvgs;
        public string[] readCloses;
        public string[] readOpens;
        public string[] readVolumes;

        public Reader(_CandleStruct[] _candleStruct)
        {
            candleStruct = _candleStruct;
            currentDirectory = Directory.GetCurrentDirectory();
            
            pathHigh    = Path.Combine(currentDirectory, @"Data\dataHigh.txt");
            pathLow     = Path.Combine(currentDirectory, @"Data\dataLow.txt");
            pathAvg     = Path.Combine(currentDirectory, @"Data\dataAvg.txt");
            pathOpen    = Path.Combine(currentDirectory, @"Data\dataOpen.txt");
            pathClose   = Path.Combine(currentDirectory, @"Data\dataClose.txt");
            pathVolume  = Path.Combine(currentDirectory, @"Data\dataVolume.txt");
        }

        /// <summary>
        /// Считать все строки из файлов распарсенных данных
        /// </summary>
        public _CandleStruct[] GetAllData()
        {
            readHeights  = File.ReadAllLines(pathHigh);
            readLows     = File.ReadAllLines(pathLow);
            readAvgs     = File.ReadAllLines(pathAvg);
            readOpens    = File.ReadAllLines(pathOpen);
            readCloses   = File.ReadAllLines(pathClose);
            readVolumes  = File.ReadAllLines(pathVolume);

            for (int i = 0; i < readHeights.Length; i++) //readHeights.Length = readLows.Length
            {
                candleStruct[i].high  = double.Parse(readHeights[i], CultureInfo.InvariantCulture);
                candleStruct[i].low   = double.Parse(readLows[i]   , CultureInfo.InvariantCulture);
                candleStruct[i].close = double.Parse(readCloses[i] , CultureInfo.InvariantCulture);
                candleStruct[i].avg   = double.Parse(readAvgs[i]   , CultureInfo.InvariantCulture);
            }

            System.Array.Resize<_CandleStruct>(ref candleStruct, readHeights.Length);

            return candleStruct;
        }
    }
}
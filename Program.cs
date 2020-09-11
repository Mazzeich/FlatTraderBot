using System;
using System.IO;
using System.Globalization;

namespace Lua
{    class Program
    {
        static void Main(string[] args)
        {
            Reader reader = new Reader();
            reader.GetAllData();

            _CandleStruct[] candles = new _CandleStruct[reader.readHeights.Length];
            for (int i = 0; i < reader.readHeights.Length; i++) //readHeights.Length = readLows.Length
            {
                candles[i].high  = double.Parse(reader.readHeights[i], CultureInfo.InvariantCulture);
                candles[i].low   = double.Parse(reader.readLows[i]   , CultureInfo.InvariantCulture);
                candles[i].close = double.Parse(reader.readCloses[i] , CultureInfo.InvariantCulture);
                candles[i].avg   = double.Parse(reader.readAvgs[i]   , CultureInfo.InvariantCulture);
            }
            
            FlatIdentifier flatIdentifier = new FlatIdentifier(candles);
            Printer printer = new Printer(flatIdentifier);
            printer.OutputApertureInfo();

            return;
        }
    }
}


using System;
using System.IO;
using System.Globalization;

namespace Lua
{    class Program
    {
        public const int n = 12000;
        static void Main(string[] args)
        {
            _CandleStruct[] candles = new _CandleStruct[n];

            Reader reader = new Reader(candles);
            candles = reader.GetAllData();
            
            FlatIdentifier flatIdentifier = new FlatIdentifier(candles);
            
            Printer printer = new Printer(flatIdentifier);
            printer.OutputApertureInfo();

            return;
        }
    }
}


using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Globalization;

namespace Lua
{
    class Program
    {
        static void Main(string[] args)
        {
            string pathOpen     = @"C:\Projects\Lua\Data\dataOpen.txt";
            string pathClose    = @"C:\Projects\Lua\Data\dataClose.txt";
            string openVolume   = @"C:\Projects\Lua\Data\dataVolume.txt";
            string pathHigh     = @"C:\Projects\Lua\Data\dataHigh.txt";
            string pathLow      = @"C:\Projects\Lua\Data\dataLow.txt";

            List<double> listHigh = new List<double>();

            string[] readText = File.ReadAllLines(pathHigh);
            //Console.WriteLine(readText.Length);
            double[] arrHigh = new double[readText.Length];

            for(int i = 0; i < readText.Length; i++)
            {
                arrHigh[i] = double.Parse(readText[i], CultureInfo.InvariantCulture);
                Console.WriteLine(readText[i] + " " + arrHigh[i]);
            }
        }

    }
}


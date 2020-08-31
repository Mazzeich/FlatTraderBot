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
            //string pathOpen = @"C:\Projects\Lua\Data\dataOpen.txt";
            //string pathClose = @"C:\Projects\Lua\Data\dataClose.txt";
            string pathHigh = @"C:\Projects\Lua\Data\dataHigh.txt";
            //string pathVolume = @"C:\Projects\Lua\Data\dataVolume.txt";
            //string pathLow = @"C:\Projects\Lua\Data\dataLow.txt";

            string[] readText = File.ReadAllLines(pathHigh);
            double[] arrHigh = new double[readText.Length];
            for (int i = 0; i < readText.Length; i++)
            {
                arrHigh[i] = double.Parse(readText[i], CultureInfo.InvariantCulture);
            }

            double globalMin = arrHigh[arrHigh.Length-1]; // Значение гМина
            double globalMax = arrHigh[arrHigh.Length-1]; // Значение гмакса
            int idxMin = 0; // Индекс найденного глобального минимума
            int idxMax = 0; // Индекс найденного глобального максимума
            double currentAvg = 0;
            for(int i = arrHigh.Length; i > 0; i--) // От 70 до 1
            {
                currentAvg += arrHigh[i-1];
                if(globalMin >= arrHigh[i-1])
                {
                    idxMin = i - 1;
                    globalMin = arrHigh[i-1];
                } else if(globalMax <= arrHigh[i-1])
                {
                    idxMax = i - 1;
                    globalMax = arrHigh[i-1];
                }
            }

            currentAvg /= arrHigh.Length;
            if((globalMax - globalMin) < (0.0005 * globalMax))
            {
                Console.WriteLine("[Ширина коридора] = {0}\nБоковик слишком узок", globalMax - globalMin);
                return;
            }

            // f(x) = kx + b
            // Нужно найти коэффициент k, стремящийся к 0, при помощи метода линейной интерполяции
            double sumX = 0; // Ось абсцисс (номер свечи)
            double sumY = 0; // Ось ординат (high свечи)
            double sumX2 = 0;
            double sumXY = 0;

            for(int i = 0; i < arrHigh.Length - 1; i++)
            {
                sumX += i;
                sumY += arrHigh[i];
                sumX2 += i*i;
                sumXY += i * arrHigh[i];
            }

            double k = ((arrHigh.Length * sumXY) - (sumX * sumY))
                        /((arrHigh.Length * sumX2) - (sumX*sumX));

            Console.WriteLine("[k] = {0}", k);
            Console.WriteLine(arrHigh[arrHigh.Length]);
            Console.WriteLine("[gMin] = {0}\n[gMax] = {1}", globalMin, globalMax);
            Console.WriteLine("[Current AVG] = {0}"     , currentAvg);
            Console.WriteLine("[arrHigh.Length] = {0}"  , arrHigh.Length);
            return;
        }

    }
}


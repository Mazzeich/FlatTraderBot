using System;

namespace Lua
{
    class Printer
    {
        private static _CandleStruct[] candles = new _CandleStruct[1000];
        private static double GMin;     // Глобальный минимум
        private static double GMax;     // Глобальный максимум 
        private static int idxGmin;     // Индекс гМина
        private static int idxGmax;     // Индекс гМакса
        private static double movAvg;   // Скользящая средняя
        private static double k;        // Угловой коэффициент апп. прямой
        private static double SDL;      // СКО по лоу
        private static double SDH;      // СКО по хай
        private static int exsNearSDL;  // Разворотов на уровне СКО-лоу
        private static int exsNearSDH;  // Разворотов на уровне СКО-хай

        private static double flatWidth; // Ширина коридора текущего периода

        public Printer(_CandleStruct[] _candles, double _GMin, double _GMax, int _idxGMin, int _idxGMax, double _movAvg, 
                        double _k, double _SDL, double _SDH, int _exsNearSDL, int _exsNearSDH)
        {
            candles = _candles;
            GMin = _GMin;
            GMax = _GMax;
            idxGmin = _idxGMin;
            idxGmax = _idxGMax;
            movAvg = _movAvg;
            k = _k;
            SDL = _SDL;
            SDH = _SDH;
            exsNearSDL = _exsNearSDL;
            exsNearSDH = _exsNearSDH;

            flatWidth = GMax - GMin;
        }

        public void OutputInfo()
        {
            Console.WriteLine("[gMin] = {0} [{1}]\t[gMax] = {2} [{3}]", GMin, idxGmin + 1, GMax, idxGmax + 1);
            Console.WriteLine("[k] = {0}", k);
            Console.WriteLine("[Скользящая средняя] = {0}", movAvg);
            Console.WriteLine("[candles.Length] = {0}", candles.Length);
            Console.WriteLine("[SDL] = {0}\t\t[SDH] = {1}", SDL, SDH);
            Console.WriteLine("[Экстремумы рядом с СКО low] = {0}\t[Экстремумы рядом с СКО high] = {1}", exsNearSDL, exsNearSDH);

            if (Math.Abs(k) < _Constants.kOffset)
            {
                Console.Write("[Ширина коридора] = {0}\t", flatWidth);
                Console.WriteLine("[Минимальная ширина коридора] = {0}\n", _Constants.minWidthCoeff * movAvg);
                Console.WriteLine("Аппроксимирующая линия почти горизонтальна. Тренд нейтральный");
                if(exsNearSDL < 2 || exsNearSDH < 2) 
                {
                    Console.WriteLine("Недостаточно вершин возле СДО!");
                } else {
                            Console.WriteLine("Цена, вероятно, формирует боковик...");
                       }
            }
            else if (k < 0)
            {
                Console.Write("[Ширина коридора] = {0}\t", flatWidth);
                Console.Write("[Минимальная ширина коридора] = {0}\n", _Constants.minWidthCoeff * movAvg);
                Console.WriteLine("Аппроксимирующая линия имеет сильный убывающий тренд");
            }
            else
            {
                Console.Write("[Ширина коридора] = {0}\nБоковик слишком узок!\t", flatWidth);
                Console.Write("[Минимальная ширина коридора] = {0}\n", _Constants.minWidthCoeff * movAvg);
                Console.WriteLine("Аппроксимирующая линия имеет сильный возрастающий тренд");
            }

            if ((flatWidth) < (_Constants.minWidthCoeff * movAvg))
            {
                Console.WriteLine("Боковик слишком узок!");
            }

            Console.WriteLine();
        }
    }
}
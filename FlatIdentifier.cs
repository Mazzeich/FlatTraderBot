using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Lua
{
    [SuppressMessage("ReSharper", "CommentTypo")]
    public class FlatIdentifier
    {
        /// <summary>
        /// Массив структур свечей
        /// </summary>
        public  List<_CandleStruct> candles = new List<_CandleStruct>();
        /// <summary>
        /// Минимум, его индекс, среднее по лоу
        /// </summary>
        private (double, int, double) lowInfo;
        /// <summary>
        /// Максимум, его индекс, среднее по хай
        /// </summary>
        private (double, int, double) highInfo;
        private  double gMin;     // Глобальный минимум
        private  double gMax;     // Глобальный максимум 
        private  int idxGmin;     // Индекс гМина
        private  int idxGmax;     // Индекс гМакса
        private  double median;   // Скользящая средняя
        private  double k;        // Угловой коэффициент апп. прямой
        private  double sdLow;    // СКО по лоу
        private  double sdHigh;   // СКО по хай
        private  int exsNearSDL;  // Разворотов на уровне СКО-лоу
        private  int exsNearSDH;  // Разворотов на уровне СКО-хай
        private Bounds bounds;
        
        public  double flatWidth; // Ширина коридора текущего периода

        public double GMin 
        {
            get { return gMin; }
            set { this.gMin = value; }
        }
        public double GMax
        {
            get { return gMax ;}
            set { this.GMax = value; }
        }
        public int IdxGmin
        {
            get { return idxGmin; }
            set { this.idxGmin = value; }
        }
        public int IdxGmax
        {
            get {return idxGmax; }
            set {this.idxGmax = value; }
        }
        public double Median
        {
            get { return median; }
            set { this.median = value; }
        }
        public double K
        {
            get { return k; }
            set { this.k = value; }
        }
        public double SDL
        {
            get { return sdLow; }
            set { this.sdLow = value; }
        }   
        public double SDH
        {
            get { return sdHigh; }
            set { this.sdHigh = value; }
        }
        public int ExsNearSDL
        {
            get { return exsNearSDL; }
            set { this.exsNearSDL = value; }
        }
        public int ExsNearSDH
        {
            get { return exsNearSDH; }
            set { this.exsNearSDH = value; }
        }

        public Bounds Bounds
        {
            get { return bounds; }
            set { this.bounds = value;  }
        }

        /// <summary>
        /// Действительно ли мы нашли боковик в заданном окне
        /// </summary>
        public bool isFlat;
        public Enum trend;
        public FlatIdentifier() {}
        public FlatIdentifier(List<_CandleStruct> _candles)
        {
            candles  = _candles;
            bounds.start = candles[0];
            bounds.end = candles[candles.Count-1];
        }

        public bool Identify()
        {
            isFlat = false;

            lowInfo  = GlobalExtremumsAndMA(false);
            highInfo = GlobalExtremumsAndMA(true);
            gMin = lowInfo.Item1;
            gMax = highInfo.Item1;
            idxGmin = lowInfo.Item2;
            idxGmax = highInfo.Item2;
            median = (highInfo.Item3 + lowInfo.Item3) * 0.5;
            flatWidth = gMax - gMin;

            k = FindK();

            (double, double) SD = StandartDeviation(median);
            sdLow = SD.Item1;
            sdHigh = SD.Item2;

            exsNearSDL = ExtremumsNearSD(median, sdLow, false);
            exsNearSDH = ExtremumsNearSD(median, sdHigh, true);

            if(Math.Abs(k) < _Constants.KOffset)
            {
                trend = Trend.Neutral;
                if(exsNearSDL > 1 && exsNearSDH > 1)
                {
                    isFlat = true;
                }
            } else if(k < 0)
            {
                trend = Trend.Down;
            } else {
                trend = Trend.Up;
            }

            return isFlat;
        }

        /// <summary>
        /// Функция поиска глобальных экстремумов в массиве структур свечей
        /// </summary>
        /// <param name="onHigh">true - ищем по high, false - по low</param>
        /// <returns></returns>
        // ReSharper disable once InconsistentNaming
        private (double, int, double) GlobalExtremumsAndMA(bool onHigh)
        {
            double globalExtremum = 0;
            double MA = 0;
            int index = 0;

            if (onHigh)
            {
                globalExtremum = double.NegativeInfinity;
                for (int i = candles.Count - 1; i >= 0; i--)
                {
                    MA += candles[i].high;
                    if (globalExtremum < candles[i].high)
                    {
                        globalExtremum = candles[i].high;
                        index = i;
                    }
                }
            }
            else
            {
                globalExtremum = double.PositiveInfinity;
                for (int i = candles.Count - 1; i >= 0; i--)
                {
                    MA += candles[i].low;
                    if (globalExtremum > candles[i].low)
                    {
                        globalExtremum = candles[i].low;
                        index = i;
                    }
                }
            }
            MA /= candles.Count;

            return (globalExtremum, index, MA);
        }

        /// <summary>
        /// Функция поиска угла наклона аппроксимирующей прямой
        /// </summary>
        /// <returns>Угловой коэффициент аппроксимирующей прямой</returns>
        private double FindK()
        {
            double k = 0;

            // Не учитывать первые и последние несколько свечей
            //`int phaseCandlesNum = (int)((double)candles.Length * _Constants.PhaseCandlesCoeff);`
            int n = candles.Count; // `-phaseCandlesNum`

            double sx = 0;
            double sy = 0;
            double sx2 = 0;
            double sxy = 0;

            for (int i = 0; i < n; i++)
            {
                sx += i;
                sy += candles[i].avg;
                sx2 += i * i;
                sxy += i * candles[i].avg;
            }
            k = ((n * sxy) - (sx * sy)) / ((n * sx2) - (sx * sx)); 

            return k;
        }

        /// <summary>
        /// Функция находит среднеквадратическое отклонение свечей тех, что выше среднего, 
        /// и тех, что ниже внутри коридора
        /// </summary>
        /// <param name="movAvg">Скользящая средняя</param>
        /// <returns></returns>
        private (double, double) StandartDeviation(double movAvg)
        {
            double sumLow = 0;
            double sumHigh = 0;

            // Количество low и high, находящихся шире минимальной ширины коридора
            int lowsCount = 0;
            int highsCount = 0;

            for (int i = 0; i < candles.Count - 1; i++)
            {
                if ((candles[i].low) <= (movAvg - _Constants.KOffset))
                {
                    sumLow += Math.Pow(movAvg - candles[i].low, 2);
                    lowsCount++;
                }
                else if ((candles[i].high) >= (movAvg + _Constants.KOffset))
                {
                    sumHigh += Math.Pow(candles[i].high - movAvg, 2);
                    highsCount++;
                }
            }
            double SDLow = Math.Sqrt(sumLow / lowsCount);
            double SDHigh = Math.Sqrt(sumHigh / highsCount);

            return (movAvg - SDLow, SDHigh + movAvg);
        }

        /// <summary>
        /// Функция, подсчитывающая количество экстремумов, находящихся поблизости СКО
        /// </summary>
        /// <param name="movAvg">Скользящая средняя</param>
        /// <param name="standartDeviation">Среднеквадратическое отклонение</param>
        /// <param name="onHigh">true - ищем по high, false - по low</param>
        /// <returns></returns>
        private int ExtremumsNearSD(double movAvg, double standartDeviation, bool onHigh)
        {
            int extremums = 0;
            double rangeToReachSD = movAvg * _Constants.SDOffset;

            if (!onHigh)
            {
                //Console.Write("[Попавшие в low индексы]: ");
                for (int i = candles.Count - 3; i > 1; i--) // Кажется, здесь есть проблема индексаций Lua и C#
                {
                    if ((Math.Abs(candles[i].low - standartDeviation) <= (rangeToReachSD)) &&
                        (candles[i].low <= candles[i-1].low) &&
                        (candles[i].low <= candles[i-2].low) &&
                        (candles[i].low <= candles[i+1].low) &&
                        (candles[i].low <= candles[i+2].low))
                    {
                        //Console.Write("{0}({1}) ", cdls[i].low, i + 1);
                        extremums++;
                        _CandleStruct temp;
                        temp = candles[i];
                        temp.low -= 0.01;
                        candles[i] = temp; // Костыль, чтобы следующая(соседняя) свеча более вероятно не подошла
                    }
                }
                //Console.WriteLine("\n[rangeToReachSD] =  {0}", rangeToReachSD);
                //Console.WriteLine("[rangeToReachSD + standartDeviation] = {0}", rangeToReachSD + standartDeviation);
            }
            else
            {
                //Console.Write("[Попавшие в high индексы]: ");
                for (int i = candles.Count - 3; i > 1; i--)
                {
                    if ((Math.Abs(candles[i].high - standartDeviation) <= (rangeToReachSD)) &&
                        (candles[i].high >= candles[i-1].high) &&
                        (candles[i].high >= candles[i-2].high) &&
                        (candles[i].high >= candles[i+1].high) &&
                        (candles[i].high >= candles[i+2].high))
                    {
                        //Console.Write("{0}({1}) ", cdls[i].high, i + 1);
                        extremums++;
                        _CandleStruct temp;
                        temp = candles[i];
                        temp.high += 0.01;
                        candles[i] = temp;
                    }
                }
                
                //Console.WriteLine("\n[rangeToReachSD] =  {0}", rangeToReachSD);
                //Console.WriteLine("[rangeToReachSD + standartDeviation] = {0}", rangeToReachSD + standartDeviation);
            }

            return extremums;
        }
    }
}
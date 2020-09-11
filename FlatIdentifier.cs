using System;

namespace Lua
{
    public class FlatIdentifier
    {
        /// <summary>
        /// Массив структур свечей
        /// </summary>
        public  _CandleStruct[] candles = new _CandleStruct[1000];
        /// <summary>
        /// Минимум, его индекс, среднее по лоу
        /// </summary>
        public  (double, int, double) lowInfo;
        /// <summary>
        /// Максимум, его индекс, среднее по хай
        /// </summary>
        public  (double, int, double) highInfo;
        public  double GMin;     // Глобальный минимум
        public  double GMax;     // Глобальный максимум 
        public  int idxGmin;     // Индекс гМина
        public  int idxGmax;     // Индекс гМакса
        public  double movAvg;   // Скользящая средняя
        public  double k;        // Угловой коэффициент апп. прямой
        public  double SDL;      // СКО по лоу
        public  double SDH;      // СКО по хай
        public  int exsNearSDL;  // Разворотов на уровне СКО-лоу
        public  int exsNearSDH;  // Разворотов на уровне СКО-хай
        public  double flatWidth; // Ширина коридора текущего периода
        /// <summary>
        /// Действительно ли мы нашли боковик в заданном окне
        /// </summary>
        public bool isFlat;
        public Enum trend;
        public FlatIdentifier(_CandleStruct[] _candles)
        {
            candles  = _candles;
            isFlat = Identify();
        }

        private  bool Identify()
        {
            bool _isFlat = false;

            lowInfo = GlobalExtremumsAndMA(false);
            highInfo = GlobalExtremumsAndMA(true);
            GMin = lowInfo.Item1;
            GMax = highInfo.Item1;
            idxGmin = lowInfo.Item2;
            idxGmax = highInfo.Item2;
            movAvg = (highInfo.Item3 + lowInfo.Item3) * 0.5;

            k = FindK();

            (double, double) SD = StandartDeviation(movAvg);
            SDL = SD.Item1;
            SDH = SD.Item2;

            exsNearSDL = ExtremumsNearSD(movAvg, SDL, false);
            exsNearSDH = ExtremumsNearSD(movAvg, SDH, true);

            if(Math.Abs(k) < _Constants.kOffset)
            {
                trend = Trend.Neutral;
                if(exsNearSDL > 1 && exsNearSDH > 1)
                {
                    _isFlat = true;
                }
            }

            return _isFlat;
        }

        /// <summary>
        /// Функция поиска глобальных экстремумов в массиве структур свечей
        /// </summary>
        /// <param name="onHigh">true - ищем по high, false - по low</param>
        /// <returns></returns>
        public  (double, int, double) GlobalExtremumsAndMA(bool onHigh)
        {
            double globalExtremum = 0;
            double MA = 0;
            int index = 0;

            if (onHigh)
            {
                globalExtremum = double.NegativeInfinity;
                for (int i = candles.Length - 1; i >= 0; i--)
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
                for (int i = candles.Length - 1; i >= 0; i--)
                {
                    MA += candles[i].low;
                    if (globalExtremum > candles[i].low)
                    {
                        globalExtremum = candles[i].low;
                        index = i;
                    }
                }
            }
            MA /= candles.Length;

            return (globalExtremum, index, MA);
        }

        /// <summary>
        /// Функция поиска угла наклона аппроксимирующей прямой
        /// </summary>
        /// <returns>Угловой коэффициент аппроксимирующей прямой</returns>
        public  double FindK()
        {
            double k = 0;

            // Не учитывать первые и последние несколько свечей
            int phaseCandlesNum = (int)((double)candles.Length * _Constants.phaseCandlesCoeff);
            int n = candles.Length - phaseCandlesNum;

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
        public  (double, double) StandartDeviation(double movAvg)
        {
            double sumLow = 0;
            double sumHigh = 0;

            // Количество low и high, находящихся шире минимальной ширины коридора
            int lowsCount = 0;
            int highsCount = 0;

            for (int i = 0; i < candles.Length - 1; i++)
            {
                if ((candles[i].low) <= (movAvg - _Constants.kOffset))
                {
                    sumLow += Math.Pow(movAvg - candles[i].low, 2);
                    lowsCount++;
                }
                else if ((candles[i].high) >= (movAvg + _Constants.kOffset))
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
        public  int ExtremumsNearSD(double movAvg, double standartDeviation, bool onHigh)
        {
            int extremums = 0;
            double rangeToReachSD = movAvg * _Constants.SDOffset;

            if (!onHigh)
            {
                //Console.Write("[Попавшие в low индексы]: ");
                for (int i = candles.Length - 3; i > 1; i--) // Кажется, здесь есть проблема индексаций Lua и C#
                {
                    if ((Math.Abs(candles[i].low - standartDeviation) <= (rangeToReachSD)) &&
                        (candles[i].low <= candles[i-1].low) &&
                        (candles[i].low <= candles[i-2].low) &&
                        (candles[i].low <= candles[i+1].low) &&
                        (candles[i].low <= candles[i+2].low))
                    {
                        //Console.Write("{0}({1}) ", cdls[i].low, i + 1);
                        extremums++;
                        candles[i].low -= 0.01; // Костыль, чтобы следующая(соседняя) свеча более вероятно не подошла
                    }
                }
                //Console.WriteLine("\n[rangeToReachSD] =  {0}", rangeToReachSD);
                //Console.WriteLine("[rangeToReachSD + standartDeviation] = {0}", rangeToReachSD + standartDeviation);
            }
            else
            {
                //Console.Write("[Попавшие в high индексы]: ");
                for (int i = candles.Length - 3; i > 1; i--)
                {
                    if ((Math.Abs(candles[i].high - standartDeviation) <= (rangeToReachSD)) &&
                        (candles[i].high >= candles[i-1].high) &&
                        (candles[i].high >= candles[i-2].high) &&
                        (candles[i].high >= candles[i+1].high) &&
                        (candles[i].high >= candles[i+2].high))
                    {
                        //Console.Write("{0}({1}) ", cdls[i].high, i + 1);
                        extremums++;
                        candles[i].high += 0.01;
                    }
                }
                
                //Console.WriteLine("\n[rangeToReachSD] =  {0}", rangeToReachSD);
                //Console.WriteLine("[rangeToReachSD + standartDeviation] = {0}", rangeToReachSD + standartDeviation);
            }

            return extremums;
        }


    }
}
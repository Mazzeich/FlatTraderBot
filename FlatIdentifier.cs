using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NLog;

namespace Lua
{
    /// <summary>
    /// Класс, реализующий определение бокового движения в заданном интервале свечей
    /// </summary>
    [SuppressMessage("ReSharper", "CommentTypo")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class FlatIdentifier
    {
        /// <summary>
        /// Логгер в logFlatIdentifier.txt
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Массив структур свечей
        /// </summary>
        public List<_CandleStruct> candles;

        private Bounds flatBounds;// Границы начала и конца найденного боковика
        
        public  double flatWidth; // Ширина коридора текущего периода
        public double gMin { get; private set; }
        public double gMax { get; private set; }
        public int idxGmin { get; private set; }
        public int idxGmax { get; private set; }
        public double average { get; private set; }
        public double k { get; private set; }
        public double SDL { get; private set; }
        public double SDH { get; private set; }
        public int exsNearSDL { get; private set; }
        public int exsNearSDH { get; private set; }
        public Bounds FlatBounds => flatBounds;
        /// <summary>
        /// Действительно ли мы нашли боковик в заданном окне
        /// </summary>
        public bool isFlat { get; private set; }
        /// <summary>
        /// Какой тренд имеет текущее окно (-1/0/1 <=> Down/Neutral/Up)
        /// </summary>
        public Enum trend;
        
        public FlatIdentifier(List<_CandleStruct> candles)
        {
            logger.Trace("\n[FlatIdentifier] initialized");
            this.candles  = candles;
            isFlat = false;
        }

        public void Identify()
        {
            logger.Trace("[Identify] started");
            
            isFlat = false;
            
            GetGlobalExtremumsAndAverage();

            flatWidth = gMax - gMin;

            FindK();

            (double low, double high) = GetStandartDeviation();
            SDL = low;
            SDH = high;
            
            EstimateExtremumsNearSD(average, SDL);
            EstimateExtremumsNearSD(average, SDH);
            
            if (Math.Abs(k) < _Constants.KOffset)
            {
                trend = Trend.Neutral;
                if ((exsNearSDL > 1) && (exsNearSDH > 1) && (flatWidth > (_Constants.MinWidthCoeff * average)))
                {
                    isFlat = true;
                }
            } else if (k < 0)
            {
                trend = Trend.Down;
                isFlat = false;
            }
            else
            {
                trend = Trend.Up;
                isFlat = false;
            }
            
            logger.Trace("[Identify] finished");
        }

        /// <summary>
        /// Функция поиска глобальных экстремумов в массиве структур свечей
        /// </summary>
        private void GetGlobalExtremumsAndAverage()
        {
            logger.Trace("Calculating global extremums and average of current aperture");
            
            average = 0;
            double averageMinis = 0;
            double averageMaxes = 0;
            
            gMin = double.PositiveInfinity;
            for (int i = 0; i < candles.Count; i++)
            {
                averageMinis += candles[i].low;
                if (gMin > candles[i].low)
                {
                    gMin = candles[i].low;
                    idxGmin = i;
                }
            }
            averageMinis /= candles.Count;

            gMax = double.NegativeInfinity;
            for (int i = 0; i < candles.Count; i++)
            {
                averageMaxes += candles[i].high;
                if (gMax < candles[i].high)
                {
                    gMax = candles[i].high;
                    idxGmax = i;
                }
            }
            averageMaxes /= candles.Count;

            average = (averageMinis + averageMaxes) * 0.5;
            
            logger.Trace("GEaA found|\t[gMin] = {0} [gMax] = {1} [average] = {2}", gMin, gMax, average);
        }

        /// <summary>
        /// Функция поиска угла наклона аппроксимирующей прямой
        /// </summary>
        /// <returns>Угловой коэффициент аппроксимирующей прямой</returns>
        private void FindK()
        {
            logger.Trace("Finding k...");
            int n = candles.Count; 

            double sumX = 0;
            double sumY = 0;
            double sumXsquared = 0;
            double sumXY = 0;

            for (int i = 0; i < n; i++)
            {
                sumX += i;
                sumY += candles[i].avg;
                sumXsquared += i * i;
                sumXY += i * candles[i].avg;
            }
            
            k = ((n * sumXY) - (sumX * sumY)) / ((n * sumXsquared) - (sumX * sumX));
            
            logger.Trace("k found. k = {0}", k);
        }
        
        /// <summary>
        /// Функция поиска угла наклона аппроксимирующей прямой
        /// без учёта последних нескольких свечей (фаза)
        /// Нужно для определения текущего тренда по инструменту
        /// </summary>
        /// <returns>Угловой коэффициент аппроксимирующей прямой</returns>
        private void FindKWithoutPhase()
        {
            logger.Trace("Finding k without phase candles...");

            // Не учитывать первые и последние несколько свечей
            int phaseCandlesNum = (int)(candles.Count * _Constants.PhaseCandlesCoeff);
            int n = candles.Count - phaseCandlesNum;

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
            
            logger.Trace("k found");
        }

        /// <summary>
        /// Функция находит среднеквадратическое отклонение свечей тех, что ниже среднего, 
        /// и тех, что выше внутри коридора
        /// </summary>
        /// <returns>double SDLow, double SDHigh</returns>
        private (double, double) GetStandartDeviation()
        {
            logger.Trace("Calculation standart deviations in current aperture...");
            double sumLow = 0;
            double sumHigh = 0;

            // Количество low и high, находящихся шире минимальной ширины коридора
            int lowsCount = 0;
            int highsCount = 0;

            for (int i = 0; i < candles.Count - 1; i++)
            {
                if ((candles[i].low) <= (average - _Constants.KOffset)) // `_average - _Constants.KOffset` ??? 
                {
                    sumLow += Math.Pow(average - candles[i].low, 2);
                    lowsCount++;
                }
                else if ((candles[i].high) >= (average + _Constants.KOffset))
                {
                    sumHigh += Math.Pow(candles[i].high - average, 2);
                    highsCount++;
                }
            }
            double SDLow = Math.Sqrt(sumLow / lowsCount);
            double SDHigh = Math.Sqrt(sumHigh / highsCount);
            logger.Trace("Standart deviations calculated. SDlow = {0} | SDhigh = {1}", average - SDLow, average + SDHigh);

            return (average - SDLow, average + SDHigh);
        }

        /// <summary>
        /// Функция, подсчитывающая количество экстремумов, находящихся поблизости СКО
        /// </summary>
        /// <param name="_average">Скользящая средняя</param>
        /// <param name="standartDeviation">Среднеквадратическое отклонение</param>
        private void EstimateExtremumsNearSD(double _average, double standartDeviation)
        {
            logger.Trace("Counting extremums near standart deviations...");
            double rangeToReachSD = _average * _Constants.SDOffset;

            //Console.Write("[Попавшие в low индексы]: ");
            for (int i = 2; i < candles.Count - 2; i++) // Кажется, здесь есть проблема индексаций Lua и C#
            {
                if (Math.Abs(candles[i].low - standartDeviation) <= rangeToReachSD &&
                    candles[i].low <= candles[i-1].low && candles[i].low <= candles[i-2].low &&
                    candles[i].low <= candles[i+1].low && candles[i].low <= candles[i+2].low)
                {
                    //Console.Write("{0}({1}) ", cdls[i].low, i + 1);
                    exsNearSDL++;
                    _CandleStruct temp = candles[i];
                    temp.low -= 0.01;
                    candles[i] = temp; // Костыль, чтобы следующая(соседняя) свеча более вероятно не подошла
                }
            }
            //Console.WriteLine("\n[rangeToReachSD] =  {0}", rangeToReachSD);
            //Console.WriteLine("[rangeToReachSD + standartDeviation] = {0}", rangeToReachSD + standartDeviation);

            //Console.Write("[Попавшие в high индексы]: ");
            for (int i = 2; i < candles.Count - 2; i++)
            {
                if (Math.Abs(candles[i].high - standartDeviation) <= rangeToReachSD &&
                    candles[i].high >= candles[i-1].high && candles[i].high >= candles[i-2].high &&
                    candles[i].high >= candles[i+1].high && candles[i].high >= candles[i+2].high)
                {
                    //Console.Write("{0}({1}) ", cdls[i].high, i + 1);
                    exsNearSDH++;
                    _CandleStruct temp = candles[i];
                    temp.high += 0.01;
                    candles[i] = temp;
                }
            }
            
            //Console.WriteLine("\n[rangeToReachSD] =  {0}", rangeToReachSD);
            //Console.WriteLine("[rangeToReachSD + standartDeviation] = {0}", rangeToReachSD + standartDeviation);
            
            logger.Trace("Extremums near SDL = {0}\tExtremums near SDH = {1}", exsNearSDL, exsNearSDH);
        }

        public Bounds SetBounds(_CandleStruct left, _CandleStruct right)
        {
            logger.Trace("Setting bounds...");
            flatBounds.left = left;
            flatBounds.right = right;
            logger.Trace("Bounds set");
            return FlatBounds;
        }
    }
}
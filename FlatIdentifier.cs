using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NLog;
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

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
        public List<_CandleStruct> candles { get; }

        private Bounds flatBounds;// Границы начала и конца найденного боковика
        
        public  double flatWidth; // Ширина коридора текущего окна
        public double gMin { get; private set; }
        public double gMax { get; private set; }
        public int idxGmin { get; private set; }
        public int idxGmax { get; private set; }
        public double mean { get; private set; }
        public double k { get; private set; }
        public double SDL { get; private set; }
        public double SDH { get; private set; }
        public double SDMean { get; private set; }
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
        /// <summary>
        /// Возможные причины того, что в текущем объекте не обнаружился нужный боковик
        /// </summary>
        public string reasonsOfApertureHasNoFlat { get; private set; }
        
        public FlatIdentifier(ref List<_CandleStruct> candles)
        {
            logger.Trace("\n[FlatIdentifier] initialized");
            this.candles  = candles;
            isFlat = false;
        }

        public void Identify()
        {
            logger.Trace("[Identify] started");
            flatBounds = SetBounds(candles[0], candles[^1]);
            logger.Trace("[{0}]: Окно с {1} по {2}", 
                flatBounds.left.date,
                flatBounds.left.time,
                flatBounds.right.time);
            
            isFlat = false;
            
            GetGlobalExtremumsAndMean(candles);
            SDMean = GetStandartDeviationMean();

            flatWidth = gMax - gMin;
            logger.Trace("[flatWidth] = {0}", flatWidth);

            // Вычисляем поле k
            k = FindK(candles);

            SDL = mean - SDMean;
            SDH = mean + SDMean;

            exsNearSDL = 0;
            exsNearSDH = 0;
            (exsNearSDL, exsNearSDH) = EstimateExtremumsNearSD(candles);
            
            if (Math.Abs(k) < _Constants.KOffset)
            {
                trend = Trend.Neutral;
                if ((exsNearSDL > 1) && (exsNearSDH > 1) && (flatWidth > (_Constants.MinWidthCoeff * mean)))
                {
                    isFlat = true;
                }
                else
                {
                    reasonsOfApertureHasNoFlat = ReasonsWhyIsNotFlat();
                }
            } else if (k < 0)
            {
                trend = Trend.Down;
                isFlat = false;
                reasonsOfApertureHasNoFlat = ReasonsWhyIsNotFlat();
            }
            else
            {
                trend = Trend.Up;
                isFlat = false;
                reasonsOfApertureHasNoFlat = ReasonsWhyIsNotFlat();
            }

            logger.Trace("isFlat = {0}\n[Identify] finished", isFlat);
        }

        /// <summary>
        /// Функция поиска глобальных экстремумов в массиве структур свечей
        /// </summary>
        private void GetGlobalExtremumsAndMean(List<_CandleStruct> candles)
        {
            logger.Trace("Calculating global extremums and [mean] of current aperture");
            
            mean = 0;

            gMin = double.PositiveInfinity;
            for (int i = 0; i < candles.Count; i++)
            {
                if (gMin > candles[i].low)
                {
                    gMin = candles[i].low;
                    idxGmin = i;
                }
            }

            gMax = double.NegativeInfinity;
            for (int i = 0; i < candles.Count; i++)
            {
                if (gMax < candles[i].high)
                {
                    gMax = candles[i].high;
                    idxGmax = i;
                }
            }

            // Вычисляем мат. ожидание
            for (int i = 0; i < candles.Count; i++)
            {
                mean += candles[i].avg;
            }

            mean /= candles.Count;
            
            logger.Trace("Global extremums and [mean] calculated.\t[gMin] = {0} [gMax] = {1} [mean] = {2}", gMin, gMax, mean);
        }

        /// <summary>
        /// Функция поиска угла наклона аппроксимирующей прямой
        /// </summary>
        /// <returns>Угловой коэффициент аппроксимирующей прямой</returns>
        private double FindK(List<_CandleStruct> candles)
        {
            logger.Trace("Finding [k]...");
            k = 0;
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
            // Точка пересечения с осью ординат

            double result = ((n * sumXY) - (sumX * sumY)) / ((n * sumXsquared) - (sumX * sumX));
            double b = (sumY - result * sumX)/n;

            logger.Trace("[k] = {0}\t [b] = {1}", result, b);
            
            return result;
        }

        /// <summary>
        /// Функция вычисляет СКО окна по avg всех свечей
        /// </summary>
        /// <returns></returns>
        private double GetStandartDeviationMean()
        {
            double sumMean = 0;
            for (int i = 0; i < candles.Count - 1; i++)
            {
                sumMean += Math.Pow(mean - candles[i].avg, 2);
            }
            double result = Math.Sqrt(sumMean / candles.Count);
            logger.Trace("[Standart Deviation on mean] = {0}", result);
            return result;
        }

        /// <summary>
        /// Функция находит среднеквадратическое отклонение свечей тех, что ниже среднего, 
        /// и тех, что выше внутри коридора
        /// </summary>
        /// <returns>double SDLow, double SDHigh</returns>
        [Obsolete("Method was used to calculate standart deviations way extreme than it should have been")]
        private (double, double) GetStandartDeviations()
        {
            logger.Trace("Calculation standart deviations in current aperture...");
            double sumLow = 0;
            double sumHigh = 0;

            // Количество low и high, находящихся шире минимальной ширины коридора
            int lowsCount = 0;
            int highsCount = 0;

            for (int i = 0; i < candles.Count - 1; i++)
            {
                if ((candles[i].low) <= mean) 
                {
                    sumLow += Math.Pow(mean - candles[i].low, 2);
                    lowsCount++;
                }
                else if ((candles[i].high) >= mean)
                {
                    sumHigh += Math.Pow(candles[i].high - mean, 2);
                    highsCount++;
                }
            }
            double SDLow = Math.Sqrt(sumLow / lowsCount);
            double SDHigh = Math.Sqrt(sumHigh / highsCount);
            logger.Trace("Standart deviations calculated. [mean] - [SDL] = {0} | [mean] + [SDH] = {1}", mean - SDLow, mean + SDHigh);

            return (mean - SDLow, mean + SDHigh);
        }

        /// <summary>
        /// Функция, подсчитывающая количество экстремумов, находящихся поблизости СКО
        /// </summary>
        private (int, int) EstimateExtremumsNearSD(List<_CandleStruct> candles)
        {
            logger.Trace("Counting extremums near standart deviations...");

            int resLow  = 0;
            int resHigh = 0;
            double distanceToSD = mean * _Constants.SDOffset;

            logger.Trace("[Попавшие в low свечи]: ");
            for (int i = 2; i < candles.Count - 2; i++) // Кажется, здесь есть проблема индексаций Lua и C#
            {
                if (Math.Abs(candles[i].low - SDL) <= distanceToSD &&
                    candles[i].low <= candles[i-1].low && candles[i].low <= candles[i-2].low &&
                    candles[i].low <= candles[i+1].low && candles[i].low <= candles[i+2].low)
                {
                    logger.Trace(candles[i].time);
                    resLow++;
                    _CandleStruct temp = candles[i];
                    temp.low -= 0.01;
                    candles[i] = temp; // Костыль, чтобы следующая(соседняя) свеча более вероятно не подошла
                }
            }
            logger.Trace("[rangeToReachSD] =  {0}", distanceToSD);
            logger.Trace("[SDL] offset = {0}|{1}", SDL - distanceToSD, SDL + distanceToSD);

            logger.Trace("[Попавшие в high свечи]: ");
            for (int i = 2; i < candles.Count - 2; i++)
            {
                if (Math.Abs(candles[i].high - SDH) <= distanceToSD &&
                    candles[i].high >= candles[i-1].high && candles[i].high >= candles[i-2].high &&
                    candles[i].high >= candles[i+1].high && candles[i].high >= candles[i+2].high)
                {
                    logger.Trace(candles[i].time);
                    resHigh++;
                    _CandleStruct temp = candles[i];
                    temp.high += 0.01;
                    candles[i] = temp;
                }
            }
            
            logger.Trace("[rangeToReachSD] =  {0}", distanceToSD);
            logger.Trace("[SDH] offset = {0}|{1}", SDH - distanceToSD, SDH + distanceToSD);
            
            logger.Trace("Extremums near SDL = {0}\tExtremums near SDH = {1}", exsNearSDL, exsNearSDH);
            return (resLow, resHigh);
        }

        private Bounds SetBounds(_CandleStruct left, _CandleStruct right)
        {
            logger.Trace("Setting bounds...");
            flatBounds.left = left;
            flatBounds.right = right;
            logger.Trace("Bounds set: [{0}] [{1}]", flatBounds.left.time, flatBounds.right.time);
            return FlatBounds;
        }
        
        /// <summary>
        /// Функция для отладки, позволяющая вывести возможные причины отсутстивия нужного бокового движения 
        /// </summary>
        /// <returns>Строка, содержащая возможные причины</returns>
        private string ReasonsWhyIsNotFlat()
        {
            string result = "";

            if ((flatWidth) < (_Constants.MinWidthCoeff * mean))
            {
                result += "Недостаточная ширина коридора. ";
            }
            if (exsNearSDL < 2)
            {
                result += "Недостаточно вершин снизу возле СКО.  ";
            }
            if (exsNearSDH < 2)
            {
                result += "Недостаточно вершин сверху возле СКО. ";
            }
            
            switch (trend)
            {
                case Trend.Down:
                {
                    result += "Нисходящий тренд. ";
                    break;
                }
                case Trend.Up:
                {
                    result += "Восходящий тренд. ";
                    break;
                }
                case Trend.Neutral:
                {
                    break;
                }
            }
            return result;
        }
    }
}
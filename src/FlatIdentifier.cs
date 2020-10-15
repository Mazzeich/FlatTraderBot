using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NLog;

namespace FlatTraderBot
{
    /// <summary>
    /// Класс, реализующий определение бокового движения в заданном интервале свечей
    /// </summary>
    [SuppressMessage("ReSharper", "CommentTypo")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class FlatIdentifier
    {
        public FlatIdentifier(ref List<_CandleStruct> candles)
        {
            logger.Trace("\n[FlatIdentifier] initialized");
            this.candles  = candles;
            isFlat = false;
        }

        public void Identify()
        {
            logger.Trace("[Identify] started");
            logger.Trace("[{0}]: Окно с {1} по {2}", candles[0].date, candles[0].time, candles[^1].time);

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
                if ((exsNearSDL > _Constants.MinExtremumsNearSD) && (exsNearSDH > _Constants.MinExtremumsNearSD) && (flatWidth > (_Constants.MinWidthCoeff * candles[^1].close)))
                {
                    isFlat = true;
                    flatBounds = SetBounds(candles[0], candles[^1]);
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
                // candles.RemoveRange(candles.Count - _Constants.ExpansionRate, _Constants.ExpansionRate);
                // flatBounds = SetBounds(candles[0], candles[^1]);
            }
            else
            {
                trend = Trend.Up;
                isFlat = false;
                reasonsOfApertureHasNoFlat = ReasonsWhyIsNotFlat();
                // candles.RemoveRange(candles.Count - _Constants.ExpansionRate, _Constants.ExpansionRate);
                // flatBounds = SetBounds(candles[0], candles[^1]);
            }

            logger.Trace("isFlat = {0}\n[Identify] finished", isFlat);
        }

        /// <summary>
        /// Функция поиска глобальных экстремумов в массиве структур свечей
        /// </summary>
        private void GetGlobalExtremumsAndMean(List<_CandleStruct> candles)
        {
            mean = 0;

            // Находим глобальный минимум
            gMin = double.PositiveInfinity;
            for (int i = 0; i < candles.Count; i++)
            {
                if (gMin > candles[i].low)
                {
                    gMin = candles[i].low;
                    idxGmin = i;
                }
            }

            // Находим глоабльный максимум
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
            
            logger.Trace("[gMin] = {0} [gMax] = {1} [mean] = {2}", 
                gMin, 
                gMax, 
                mean);
        }

        /// <summary>
        /// Функция поиска угла наклона аппроксимирующей прямой
        /// </summary>
        /// <returns>Угловой коэффициент аппроксимирующей прямой</returns>
        private double FindK(List<_CandleStruct> candles)
        {
            // https://prog-cpp.ru/mnk/
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
            double sumLow = 0;
            double sumHigh = 0;

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
            logger.Trace("[SDL] - offset = {0}|[SDH] + offset = {1}", SDL - distanceToSD, SDL + distanceToSD);

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
            
            logger.Trace("Extremums near SDL = {0}\tExtremums near SDH = {1}", resLow, resHigh);
            return (resLow, resHigh);
        }

        private _Bounds SetBounds(_CandleStruct left, _CandleStruct right)
        {
            logger.Trace("Setting bounds...");
            _Bounds result = flatBounds;
            result.left = left;
            result.right = right;
            logger.Trace("_Bounds set: [{0}][{1}] [{2}][{3}]", 
                result.left.time,
                result.left.index,
                result.right.time,
                result.right.index);
            return result;
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
        
        /// <summary>
        /// Логгер в logFlatIdentifier.txt
        /// </summary>
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Массив структур свечей
        /// </summary>
        public List<_CandleStruct> candles { get; }
        /// <summary>
        /// Границы начала и конца найденного боковика
        /// </summary>
        public _Bounds flatBounds { get; private set; }
        /// <summary>
        /// Ширина текущего коридора
        /// </summary>
        public  double flatWidth;
        /// <summary>
        /// Глобальный минимум в боковике
        /// </summary>
        public double gMin { get; private set; }
        /// <summary>
        /// Глобальный максимум в боковике
        /// </summary>
        public double gMax { get; private set; }
        /// <summary>
        /// Индекс глобального минимума относительно окна
        /// </summary>
        public int idxGmin { get; private set; }
        /// <summary>
        /// Индекс глобального максимума относительно окна
        /// </summary>
        public int idxGmax { get; private set; }
        /// <summary>
        /// Значение математического ожидания в боковике
        /// </summary>
        public double mean { get; private set; }
        /// <summary>
        /// Угловой коэффициент аппроксимирущей прямой
        /// </summary>
        public double k { get; private set; }
        /// <summary>
        /// Среднеквадратическое отклонение снизу
        /// </summary>
        public double SDL { get; private set; }
        /// <summary>
        /// Среднеквадратическое отклонение сверху
        /// </summary>
        public double SDH { get; private set; }
        /// <summary>
        /// Среднеквадратическое отклонение в боковике
        /// </summary>
        public double SDMean { get; private set; }
        /// <summary>
        /// Количество экстремумов возле среднеквадратического отклонения снизу
        /// </summary>
        public int exsNearSDL { get; private set; }
        /// <summary>
        /// Количество экстремумов возле среднеквадратического отклонения сверху
        /// </summary>
        public int exsNearSDH { get; private set; }
        /// <summary>
        /// Действительно ли мы нашли боковик в заданном окне
        /// </summary>
        public bool isFlat { get; private set; }
        /// <summary>
        /// Какой тренд имеет текущее окно (-1/0/1 <=> Down/Neutral/Up)
        /// </summary>
        public Trend trend;
        /// <summary>
        /// Возможные причины того, что в текущем объекте не обнаружился нужный боковик
        /// </summary>
        public string reasonsOfApertureHasNoFlat { get; private set; }
        /// <summary>
        /// Снизу или сверху сформировался боковик
        /// </summary>
        public FormedFrom formedFrom { get; set; }
    }
}
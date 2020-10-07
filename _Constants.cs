// ReSharper disable CommentTypo
namespace Lua
{
    /// <summary>
    /// Структура постоянных величин
    /// </summary>
    public struct _Constants
    {
        /// <value> Размер окна </value>
        public const int NAperture = 60;

        /// <value> На сколько свечей увеличивать окно  </value>
        public const int ExpansionValue = 2;
    
        /// <value> Минимальная ширина коридора (коэфф. от цены инструмента)</value>
        public const double MinWidthCoeff = 0.002;

        /// <value> Коэффициент для определения поведения тренда</value>
        public const double KOffset = 0.001;

        /// <value> Возможное отклонение экстремума от линии СКО</value>
        public const double SDOffset = 0.0006;

        /// <value> Количество фазовых свечей, которые не нужно учитывать при вычислении уголового коэффициента </value>
        public const double PhaseCandlesCoeff = 0.05;
    }
}
// ReSharper disable CommentTypo
namespace Lua
{
    /// <summary>
    /// Структура постоянных величин
    /// </summary>
    public struct _Constants
    {
        /// <summary>
        /// Размер окна
        /// </summary>
        public const int NAperture = 50;

        /// <summary>
        /// На сколько свечей увеличивать окно
        /// </summary>
        public const int ExpansionValue = 2;
    
        /// <summary>
        /// Минимальная ширина коридора (коэфф. от цены инструмента)
        /// </summary>
        public const double MinWidthCoeff = 0.002;

        /// <summary>
        /// Коэффициент для определения поведения тренда
        /// </summary>
        public const double KOffset = 0.001;

        /// <summary>
        /// Возможное отклонение экстремума от линии СКО
        /// </summary>
        public const double SDOffset = 0.0006;

        /// <summary>
        /// Количество фазовых свечей, которые не нужно учитывать при вычислении уголового коэффициента
        /// </summary>
        public const double PhaseCandlesCoeff = 0.05;
    }
}
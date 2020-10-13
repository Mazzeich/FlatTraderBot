// ReSharper disable CommentTypo
namespace FlatTraderBot
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
        /// Минимальное количество вершин сверху или снизу возле СКО
        /// </summary>
        public const int MinExtremumsNearSD = 3;

        /// <summary>
        /// Минимальное расстояние между боковиками, чтобы не склеивать их в один
        /// </summary>
        public const int MinFlatGap = 5;

        /// <summary>
        /// На сколько свечей увеличивать окно
        /// </summary>
        public const int ExpansionRate = 3;

        /// <summary>
        /// Максимально возможное количество свечей между экстремумом и боковиком
        /// </summary>
        public const int MaxFlatExtremumDistance = 100;
    
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
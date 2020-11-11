namespace FlatTraderBot.Structs
{
    /// <summary> Структура постоянных величин </summary>
    public struct _Constants
    {
        /// <summary> На сколько свечей увеличивать окно </summary>
        public const int ExpansionRate = 6;
        
        /// <summary> Размер окна </summary>
        public const int NAperture = 50;
        
        /// <summary> Минимальное количество вершин сверху или снизу возле СКО </summary>
        public const int MinExtremumsNearSD = 3;
        
        /// <summary> Минимальное расстояние между боковиками, чтобы не склеивать их в один </summary>
        public const int MinFlatGap = 60;
        
        /// <summary> Максимально возможное количество свечей между экстремумом и боковиком </summary>
        public const int MaxFlatLeftExtremumDistance = 100;
        
        /// <summary> Сколько активов участвует в одной сделке </summary>
        public const int NumberOfAssetsForDeal = 100;
        
        /// <summary> Минимальная ширина коридора (коэфф. от цены инструмента) </summary>
        public const double MinWidthCoeff = 0.001125;
        
        /// <summary> Коэффициент для определения поведения тренда </summary>
        public const double KOffset = 0.0025;
        
        /// <summary> Возможное отклонение экстремума от линии СКО </summary>
        public const double SDOffset = 0.0005;
        
        /// <summary> Отклонение свечи от боковика для фиксирования закрытия </summary>
        public const double LeavingCoeff = 0.004;

        /// <summary> Необходимое отклонение от цены, чтобы можно было учесть экстремум перед боковиком </summary>
        public const double FlatClassifyOffset = 0.0005;
        
        /// <summary> Возможное отклонение мат. ожиданий боковиков для их объединения </summary>
        public const double FlatsMeanOffset = 0.0006;
        
        /// <summary> Стартовый баланс робота </summary>
        public const double InitialBalance = 100000;

        /// <summary> Множитель для выставления тейк-профита на заявке </summary>
        public const double TakeProfitPriceCoeff = 0.005; //0.005
    }
}
namespace FlatTraderBot.Structs
{
    /// <summary> Структура постоянных величин </summary>
    public static class _Constants
    {
        /// <summary> На сколько расширять окно </summary>
        // public static int ExpansionRate = 2; 
        public static int ExpansionRate = 6; 
        
        /// <summary> Размер окна </summary>
        // public static int NAperture = 20;
        public static int NAperture = 50;
        
        /// <summary> Минимальное количество вершин сверху или снизу возле СКО </summary>
        // public static int MinExtremumsNearSD = 1;
        public static int MinExtremumsNearSD = 3;
        
        /// <summary> Минимальное расстояние между боковиками, чтобы не склеивать их в один </summary>
        // public static int MinFlatGap = 60;
        public static int MinFlatGap = 60;
        
        /// <summary> Максимально возможное количество свечей между экстремумом и боковиком </summary>
        // public static int MaxFlatLeftExtremumDistance = 100;
        public static int MaxFlatLeftExtremumDistance = 100;

        /// <summary> Минимальное количество найденных флетов для анализа </summary>
        // public static int MinFlatsMustBeFound = 3;
        public static int MinFlatsMustBeFound = 3;

        /// <summary> Порог для классификации склона в качестве плавного или крутого </summary>
        // public static double ArcTanThreshold = 1.5;
        public static double ArcTanThreshold = 1.5;

        /// <summary> Необходимое отклонение от цены, чтобы можно было учесть экстремум перед боковиком </summary>
        // public static double FlatClassifyOffset = 0.0005;
        public static double FlatClassifyOffset = 0.0005;

        /// <summary> Возможное отклонение мат. ожиданий боковиков для их объединения </summary>
        // public static double FlatsMeanOffset = 0.0006;
        public static double FlatsMeanOffset = 0.0006;

        /// <summary> Стартовый баланс робота </summary>
        // public static double InitialBalance = 0;
        public static double InitialBalance = 1000;

        /// <summary> Коэффициент для определения поведения тренда </summary>
        // public static double KOffset = 0.0025;
        public static double KOffset = 0.0025;

        /// <summary> Отклонение свечи от боковика для фиксирования выхода </summary>
        // public static double LeavingCoeff = 0.008;
        public static double LeavingCoeff = 0.004;

        /// <summary> Минимальная ширина коридора (коэфф. от цены инструмента) </summary>
        // public static double MinWidthCoeff = 0.002;
        public static double MinWidthCoeff = 0.002;

        /// <summary> Сколько активов участвует в одной сделке </summary>
        // public static double NumberOfAssetsForDeal = 1;
        public static double NumberOfAssetsForDeal = 1;

        /// <summary> Сколько СК отклонений отсчитывать от мат. ожидания </summary>
        public static double SDAmount = 1;
        // public static double SDAmount = 3;

        /// <summary> Возможное отклонение экстремума от линии СКО </summary>
        // public static double SDOffset = 0.002;
        public static double SDOffset = 0.0005;

        /// <summary> Множитель для выставления тейк-профита на заявке </summary>
        // public static double TakeProfitPriceCoeff = 0.0005;
        public static double TakeProfitPriceCoeff = 0.005;
    }
}
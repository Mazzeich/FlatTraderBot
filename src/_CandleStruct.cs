namespace FlatTraderBot
{
    /// <summary>
    /// Структура свечи (high, low, close, avg, date)
    /// </summary>
    public struct _CandleStruct
    {
        /// <summary>
        /// Индекс свечи относительно глобальных данных
        /// </summary>
        public int index;
        /// <summary>
        /// Индекс свечи внутри одного дня торгов
        /// </summary>
        public int intradayIndex;
        /// <summary>
        /// Цена открытия текущей свечи
        /// </summary>
        public double open;
        /// <summary>
        /// Хай текущей свечи
        /// </summary>
        public double high; 
        /// <summary>
        /// Лоу текущей свечи
        /// </summary>
        public double low;
        /// <summary>
        /// Цена закрытия текущей свеич
        /// </summary>
        public double close;
        /// <summary>
        /// Средняя цена по свече (= (хай + лоу)/*0.5)
        /// </summary>
        public double avg;
        /// <summary>
        /// Дата закрытия текущей свечи
        /// </summary>
        public string date;
        /// <summary>
        /// Время закрытия текущей свечи
        /// </summary>
        public string time;
    }
}
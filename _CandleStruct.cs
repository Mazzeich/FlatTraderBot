// ReSharper disable CommentTypo
namespace Lua
{
    /// <summary>
    /// Структура свечи (high, low, close, avg, date)
    /// </summary>
    public struct _CandleStruct
    {
        /// <value> Хай текущей свечи </value>
        public double high;
        /// <value> Лоу текущей свечи </value>
        public double low;
        /// <value> Цена закрытия текущей свечи </value>
        public double close;
        /// <value> Средняя цена по свече (= (хай - лоу)/*0.5) </value>
        public double avg;
    
        /// <value> Дата закрытия свечи </value>
        public string date;
    }
}
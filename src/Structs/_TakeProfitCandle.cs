namespace FlatTraderBot.Structs
{
	/// <summary>
	/// Структура свечи дальнего пробоя после боковика
	/// </summary>
	public struct _TakeProfitCandle
	{
		/// <summary>
		/// Сама свеча пробоя
		/// </summary>
		public _CandleStruct candle;
		/// <summary>
		/// Расстояние в свечах между свечой закрытия и самой свечой пробоя
		/// </summary>
		public int distanceToLeave;
		/// <summary>
		/// Разница в цене между ценой закрытия свечи закрытия боковика и самой свечой пробоя
		/// </summary>
		public double deltaPriceTakeProfitToLeave;
	}
}
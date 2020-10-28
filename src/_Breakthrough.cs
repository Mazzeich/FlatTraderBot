namespace FlatTraderBot
{
	/// <summary>
	/// Структура свечи дальнего пробоя после боковика
	/// </summary>
	public struct _Breakthrough
	{
		/// <summary>
		/// Сама свеча пробоя
		/// </summary>
		public _CandleStruct candle;
		/// <summary>
		/// Расстояние в свечах между свечой закрытия и самой свечой пробоя
		/// </summary>
		public int distanceToClose;
		/// <summary>
		/// Разница в цене между ценой закрытия свечи закрытия боковика и самой свечой пробоя
		/// </summary>
		public double deltaPriceBreakthroughToClose;
	}
}
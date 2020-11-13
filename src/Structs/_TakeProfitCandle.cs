namespace FlatTraderBot.Structs
{
	/// <summary>
	/// Структура тейк-профита флета (используется для сбора статистики)
	/// </summary>
	public struct _TakeProfitCandle
	{
		/// <summary>
		/// Свеча тейк-профита
		/// </summary>
		public _CandleStruct candle;
		/// <summary>
		/// Расстояние в свечах между свечой закрытия и самой свечой пробоя
		/// </summary>
		public int deltaDistance;
		/// <summary>
		/// Разница в цене между ценой закрытия свечи закрытия боковика и самой свечой пробоя
		/// </summary>
		public double deltaPrice;
	}
}
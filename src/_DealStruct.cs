namespace FlatTraderBot
{
	/// <summary>
	/// Структура совершённой сделки
	/// </summary>
	public struct _DealStruct
	{
		/// <summary>
		/// Свеча открытия сделки
		/// </summary>
		public _CandleStruct OpenCandle;
		/// <summary>
		/// Свеча закрытия сделки
		/// </summary>
		public _CandleStruct CloseCandle;
		/// <summary>
		/// Тип сделки (шорт, лонг)
		/// </summary>
		public char type;
		/// <summary>
		/// Прибыль завершённой сделки
		/// </summary>
		public double profit;
	}
}
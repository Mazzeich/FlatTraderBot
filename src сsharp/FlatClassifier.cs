using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Candles
{
	public class FlatClassifier
	{
		/// <summary>
		/// Логгер
		/// </summary>
		private readonly Logger logger = LogManager.GetCurrentClassLogger();
		/// <summary>
		/// Список всех найденных боковиков
		/// </summary>
		private List<FlatIdentifier> flatCollection = new List<FlatIdentifier>();
		/// <summary>
		/// Глобальный список свечей
		/// </summary>
		private List<_CandleStruct> globalCandles = new List<_CandleStruct>();
		/// <summary>
		/// Всего боковиков
		/// </summary>
		private int flatsOverall;
		/// <summary>
		/// Сколько боковиков сформировано после падения
		/// </summary>
		private int flatsFromDescension;
		/// <summary>
		/// Сколько боковиков сформировано после взлёта
		/// </summary>
		private int flatsFromAscension;

		private FlatClassifier()
		{
			logger.Trace("[FlatClassifier] initialized");	
		}

		public FlatClassifier(List<FlatIdentifier> flats, List<_CandleStruct> candles) : this()
		{
			this.flatCollection = flats;
			globalCandles = candles;
			flatsOverall = flatCollection.Count;
		}

		public void ClassifyAllFlats()
		{
			logger.Trace("Classification started...");

			int fromAscending = 0;
			int fromDescending = 0;
			for (int i = 0; i < flatsOverall; i++)
			{
				Enum flatFormedFrom = Classify(flatCollection[i], i);
				switch (flatFormedFrom)
				{
					case (FormedFrom.Ascending):
					{
						fromAscending++;
						break;
					}
					case (FormedFrom.Descending):
					{
						fromDescending++;
						break;
					}
					default:
						break;
				}
			}

			logger.Trace("From ascending = {0} | From descending = {1}", fromAscending, fromDescending);
		}

		private Enum Classify(FlatIdentifier flatIdentifier, int flatNumber)
		{
			_CandleStruct closesExtremum = ClosestExtremum(flatNumber);
			return closesExtremum.avg > flatIdentifier.mean ? FormedFrom.Ascending : FormedFrom.Descending;
		}

		private _CandleStruct ClosestExtremum(int flatNumber)
		{
			_CandleStruct closestMinimum = FindClosestMinimum(flatNumber);
			_CandleStruct closestMaximum = FindClosestMaximum(flatNumber);
			
			logger.Trace("Closest [Min] is at {0} | Closest [Max] is at {1}", closestMinimum.time, closestMaximum.time);

			return closestMaximum.index > closestMinimum.index ? closestMaximum : closestMinimum;
		}

		/// <summary>
		/// Функция находит ближаший к боковику минимум, экстремальнее глобального в боковике
		/// </summary>
		/// <returns>Свеча</returns>
		private _CandleStruct FindClosestMinimum(int flatNumber)
		{
			_CandleStruct temp = globalCandles[0];
			bool found = false;
			int candlesPassed = 0;
			while (!found)
			{
				temp = globalCandles[flatCollection[flatNumber].flatBounds.leftBound.index - candlesPassed];
				if (temp.low < flatCollection[flatNumber].gMin &&
				    temp.low < globalCandles[flatCollection[flatNumber].flatBounds.leftBound.index - candlesPassed - 2].low &&
				    temp.low < globalCandles[flatCollection[flatNumber].flatBounds.leftBound.index - candlesPassed - 1].low &&
				    temp.low < globalCandles[flatCollection[flatNumber].flatBounds.leftBound.index - candlesPassed + 1].low &&
				    temp.low < globalCandles[flatCollection[flatNumber].flatBounds.leftBound.index - candlesPassed + 2].low)
				{
					found = true;
				}

				candlesPassed++;
			}
			return temp;
		}

		/// <summary>
		/// Функция находит ближаший к боковику максимум, экстремальнее глобального в боковике
		/// </summary>
		/// <returns>Свеча</returns>
		private _CandleStruct FindClosestMaximum(int flatNumber)
		{
			_CandleStruct temp = globalCandles[0];
			bool found = false;
			int candlesPassed = 0;
			while (!found)
			{
				temp = globalCandles[flatCollection[flatNumber].flatBounds.leftBound.index - candlesPassed];
				if (temp.high > flatCollection[flatNumber].gMax &&
				    temp.high > globalCandles[flatCollection[flatNumber].flatBounds.leftBound.index - candlesPassed - 2].high &&
				    temp.high > globalCandles[flatCollection[flatNumber].flatBounds.leftBound.index - candlesPassed - 1].high &&
				    temp.high > globalCandles[flatCollection[flatNumber].flatBounds.leftBound.index - candlesPassed + 1].high &&
				    temp.high > globalCandles[flatCollection[flatNumber].flatBounds.leftBound.index - candlesPassed + 2].high)
				{
					found = true;
				}

				candlesPassed++;
			}
			return temp;
		}
	}
}
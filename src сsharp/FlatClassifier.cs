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
						logger.Trace("[{0}]: {1} from Asceding", flatCollection[i].flatBounds.left.date, flatCollection[i].flatBounds.left.time);
						fromAscending++;
						break;
					}
					case (FormedFrom.Descending):
					{
						logger.Trace("[{0}]: {1} from Descending", flatCollection[i].flatBounds.left.date, flatCollection[i].flatBounds.left.time);
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
			_CandleStruct closestExtremum = ClosestExtremum(flatNumber);
			return closestExtremum.avg > flatIdentifier.mean ? FormedFrom.Ascending : FormedFrom.Descending;
		}

		/// <summary>
		/// Функция находит ближайший эксремум, начиная поиск с левого края окна
		/// </summary>
		/// <param name="flatNumber">Номер объекта в списке боковиков</param>
		/// <returns>Свеча</returns>
		private _CandleStruct ClosestExtremum(int flatNumber)
		{
			int candlesPassed = 0;
			while (candlesPassed < _Constants.MaxFlatExtremumDistance)
			{
				_CandleStruct closestExtremum = globalCandles[flatCollection[flatNumber].flatBounds.left.index - candlesPassed];

				if (closestExtremum.low < flatCollection[flatNumber].gMin &&
				    closestExtremum.low < globalCandles[flatCollection[flatNumber].flatBounds.left.index - candlesPassed - 2].low &&
				    closestExtremum.low < globalCandles[flatCollection[flatNumber].flatBounds.left.index - candlesPassed - 1].low &&
				    closestExtremum.low < globalCandles[flatCollection[flatNumber].flatBounds.left.index - candlesPassed + 1].low &&
				    closestExtremum.low < globalCandles[flatCollection[flatNumber].flatBounds.left.index - candlesPassed - 2].low)
				{
					return closestExtremum;
				}
				else if (closestExtremum.high > flatCollection[flatNumber].gMax &&
				         closestExtremum.high > globalCandles[flatCollection[flatNumber].flatBounds.left.index - candlesPassed - 2].high &&
				         closestExtremum.high > globalCandles[flatCollection[flatNumber].flatBounds.left.index - candlesPassed - 1].high &&
				         closestExtremum.high > globalCandles[flatCollection[flatNumber].flatBounds.left.index - candlesPassed + 1].high &&
				         closestExtremum.high > globalCandles[flatCollection[flatNumber].flatBounds.left.index - candlesPassed - 2].high)
				{
					return closestExtremum;
				}
				else
				{
					candlesPassed++;
				}
			}

			return globalCandles[0];
		}
	}
}
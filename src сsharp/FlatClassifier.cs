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

		/// <summary>
		/// Функция, задающая поле formedFrom объекта класса FlatIdentifier
		/// </summary>
		/// <param name="flatIdentifier">Боковик</param>
		/// <param name="flatNumber">Номер боковика</param>
		/// <returns>Enum FormedFrom</returns>
		private FormedFrom Classify(FlatIdentifier flatIdentifier, int flatNumber)
		{
			_CandleStruct closestExtremum = ClosestExtremum(flatNumber);
			if (closestExtremum.avg > flatIdentifier.mean)
			{
				flatIdentifier.formedFrom = FormedFrom.Ascending;
				return FormedFrom.Ascending;
			}
			else
			{
				flatIdentifier.formedFrom = FormedFrom.Descending;
				return FormedFrom.Descending;
			}
		}

		/// <summary>
		/// Функция находит ближайший экстремум, начиная поиск с левого края окна
		/// </summary>
		/// <param name="flatNumber">Номер объекта в списке боковиков</param>
		/// <returns>Свеча</returns>
		private _CandleStruct ClosestExtremum(int flatNumber)
		{
			int candlesPassed = 0;
			FlatIdentifier currentFlat = flatCollection[flatNumber];

			while (candlesPassed < _Constants.MaxFlatExtremumDistance)
			{
				_CandleStruct closestExtremum = globalCandles[currentFlat.flatBounds.left.index - candlesPassed];

				if (closestExtremum.low < currentFlat.gMin &&
				    closestExtremum.low < globalCandles[currentFlat.flatBounds.left.index - candlesPassed - 2].low &&
				    closestExtremum.low < globalCandles[currentFlat.flatBounds.left.index - candlesPassed - 1].low &&
				    closestExtremum.low < globalCandles[currentFlat.flatBounds.left.index - candlesPassed + 1].low &&
				    closestExtremum.low < globalCandles[currentFlat.flatBounds.left.index - candlesPassed - 2].low)
				{
					return closestExtremum;
				}
				else if (closestExtremum.high > currentFlat.gMax &&
				         closestExtremum.high > globalCandles[currentFlat.flatBounds.left.index - candlesPassed - 2].high &&
				         closestExtremum.high > globalCandles[currentFlat.flatBounds.left.index - candlesPassed - 1].high &&
				         closestExtremum.high > globalCandles[currentFlat.flatBounds.left.index - candlesPassed + 1].high &&
				         closestExtremum.high > globalCandles[currentFlat.flatBounds.left.index - candlesPassed - 2].high)
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
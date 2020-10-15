using System;
using System.Collections.Generic;
using NLog;

namespace FlatTraderBot
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
		private readonly List<FlatIdentifier> flatCollection;
		/// <summary>
		/// Глобальный список свечей
		/// </summary>
		private readonly List<_CandleStruct> globalCandles;
		/// <summary>
		/// Всего боковиков
		/// </summary>
		private readonly int flatsOverall;
		/// <summary>
		/// Сколько боковиков сформировано после падения
		/// </summary>
		private int flatsFromDescension;
		/// <summary>
		/// Сколько боковиков сформировано после взлёта
		/// </summary>
		private int flatsFromAscension;
		/// <summary>
		/// Средний интервал между боковиками
		/// </summary>
		private double meanFlatInterval;
		/// <summary>
		/// Средний интервал между концом боковика и предстоящим отклоенением
		/// </summary>
		private double meanOffsetDistance;

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

			for (int i = 0; i < flatsOverall; i++)
			{
				Enum flatFormedFrom = Classify(flatCollection[i], i);
				switch (flatFormedFrom)
				{
					case (FormedFrom.Ascending):
					{
						logger.Trace("[{0}]: {1} from Asceding", flatCollection[i].flatBounds.left.date, flatCollection[i].flatBounds.left.time);
						flatsFromAscension++;
						break;
					}
					case (FormedFrom.Descending):
					{
						logger.Trace("[{0}]: {1} from Descending", flatCollection[i].flatBounds.left.date, flatCollection[i].flatBounds.left.time);
						flatsFromDescension++;
						break;
					}
					default:
						break;
				}
			}

			logger.Trace("From ascending = {0} | From descending = {1}", flatsFromAscension, flatsFromDescension);
			logger.Trace("[fromAscening/fromDescending] = {0}%/{1}%", 
				flatsFromAscension * 100/ flatsOverall,
				flatsFromDescension * 100/ flatsOverall);

			meanFlatInterval = CalculateMeanInterval(flatCollection);
			logger.Trace("[meanFlatInterval] = {0}", meanFlatInterval);

			meanOffsetDistance = CalculateMeanOffsetDistance(flatCollection, globalCandles);
			logger.Trace("[meanOffsetDistance] = {0}", meanOffsetDistance);
		}

		/// <summary>
		/// Функция, задающая поле formedFrom объекта класса FlatIdentifier
		/// </summary>
		/// <param name="flatIdentifier">Боковик</param>
		/// <param name="flatNumber">Номер боковика</param>
		/// <returns>Enum FormedFrom</returns>
		private FormedFrom Classify(FlatIdentifier flatIdentifier, int flatNumber)
		{
			_CandleStruct closestExtremum = FindClosestExtremum(flatNumber);
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
		private _CandleStruct FindClosestExtremum(int flatNumber)
		{
			int candlesPassed = 0;
			FlatIdentifier currentFlat = flatCollection[flatNumber];

			// Цикл выполняется, пока на найдётся подходящий экстремум либо не пройдёт константное число итераций
			while (candlesPassed < _Constants.MaxFlatExtremumDistance)
			{
				_CandleStruct closestExtremum = globalCandles[currentFlat.flatBounds.left.index - candlesPassed];

				if (closestExtremum.low < currentFlat.gMin - _Constants.flatClassifyOffset * currentFlat.gMin &&
				    closestExtremum.low < globalCandles[currentFlat.flatBounds.left.index - candlesPassed - 2].low &&
				    closestExtremum.low < globalCandles[currentFlat.flatBounds.left.index - candlesPassed - 1].low &&
				    closestExtremum.low < globalCandles[currentFlat.flatBounds.left.index - candlesPassed + 1].low &&
				    closestExtremum.low < globalCandles[currentFlat.flatBounds.left.index - candlesPassed - 2].low)
				{
					return closestExtremum;
				}
				else if (closestExtremum.high > currentFlat.gMax + _Constants.flatClassifyOffset * currentFlat.gMax &&
				         closestExtremum.high > globalCandles[currentFlat.flatBounds.left.index - candlesPassed - 2].high &&
				         closestExtremum.high > globalCandles[currentFlat.flatBounds.left.index - candlesPassed - 1].high &&
				         closestExtremum.high > globalCandles[currentFlat.flatBounds.left.index - candlesPassed + 1].high &&
				         closestExtremum.high > globalCandles[currentFlat.flatBounds.left.index - candlesPassed - 2].high)
				{
					return closestExtremum;
				}
				candlesPassed++;
			}

			if (candlesPassed == 100)
			{
				logger.Trace("Extremum haven't found");
			}

			return globalCandles[0];
		}

		/// <summary>
		/// Функция вычисляет среднее расстояние между боковиками
		/// </summary>
		/// <param name="flatIdentifiers">Коллекция боковиков</param>
		/// <returns>Средний интервал</returns>
		private double CalculateMeanInterval(List<FlatIdentifier> flatIdentifiers)
		{
			double meanDistance = 0;
			for (int i = 1; i < flatIdentifiers.Count; i++)
			{
				double gap = flatIdentifiers[i].flatBounds.left.index - flatIdentifiers[i - 1].flatBounds.right.index;
				meanDistance += gap;
			}

			meanDistance /= flatIdentifiers.Count - 1;
			return meanDistance;
		}

		/// <summary>
		/// Функция вычисляет среднее расстояние между боковиками и их предстоящими отклонениями
		/// </summary>
		/// <param name="flatIdentifiers"></param>
		/// <returns></returns>
		private double CalculateMeanOffsetDistance(List<FlatIdentifier> flatIdentifiers, List<_CandleStruct> candleStructs)
		{
			double meanDistance = 0;
			double distance = 0;

			for (int i = 0; i < flatIdentifiers.Count - 1; i++)
			{
				for (int j = flatIdentifiers[i].flatBounds.right.index + 1; j < flatIdentifiers[i + 1].flatBounds.left.index; j++)
				{
					double currentOffset = Math.Abs(candleStructs[j].avg - flatIdentifiers[i].mean);
					if (currentOffset >= flatIdentifiers[i].flatWidth)
					{
						distance = candleStructs[j].index - flatIdentifiers[i].flatBounds.right.index;
					} else continue;
				}

				meanDistance += distance;
			}

			meanDistance /= flatIdentifiers.Count - 1;
			return meanDistance;
		}
	}
}
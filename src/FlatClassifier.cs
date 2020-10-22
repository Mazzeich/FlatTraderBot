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
		{ }

		public FlatClassifier(List<FlatIdentifier> flats, List<_CandleStruct> candles) : this()
		{
			flatCollection = new List<FlatIdentifier>(flats);
			globalCandles = candles;
			flatsOverall = flatCollection.Count;
		}

		/// <summary>
		/// Функция, запускающая анализ флетов
		/// </summary>
		public void ClassifyAllFlats()
		{
			for (int i = 0; i < flatsOverall; i++)
			{
				Enum flatFormedFrom = Classify(flatCollection[i], i);
				switch (flatFormedFrom)
				{
					case (FormedFrom.Ascending):
					{
						logger.Trace($"[{flatCollection[i].flatBounds.left.date}]: {flatCollection[i].flatBounds.left.time} from asceding");
						flatsFromAscension++;
						break;
					}
					case (FormedFrom.Descending):
					{
						logger.Trace($"[{flatCollection[i].flatBounds.left.date}]: {flatCollection[i].flatBounds.left.time} from descending");
						flatsFromDescension++;
						break;
					}
					default:
						break;
				}
			}

			int flatsFromAscensionPercantage = flatsFromAscension * 100 / flatsOverall;
			int flatsFromDescensionPercentage = flatsFromDescension * 100 / flatsOverall;

			logger.Trace($"From ascending = {flatsFromAscension} | From descending = {flatsFromDescension}");
			logger.Trace($"[fromAscening/fromDescending] = {flatsFromAscensionPercantage}%/{flatsFromDescensionPercentage}%");

			meanFlatInterval = CalculateMeanInterval(flatCollection);
			logger.Trace($"[meanFlatInterval] = {meanFlatInterval}");

			meanOffsetDistance = CalculateMeanOffsetDistance(flatCollection, globalCandles);
			logger.Trace($"[meanOffsetDistance] = {meanOffsetDistance}");
		}

		/// <summary>
		/// Функция, задающая поле formedFrom объекта класса FlatIdentifier
		/// </summary>
		/// <param name="flatIdentifier">Боковик</param>
		/// <param name="flatNumber">Номер боковика</param>
		/// <returns>Enum FormedFrom</returns>
		private FormedFrom Classify(FlatIdentifier flatIdentifier, int flatNumber)
		{
			FormedFrom result;
			_CandleStruct closestExtremum = FindClosestExtremum(flatNumber);
			if (closestExtremum.avg > flatIdentifier.mean)
			{
				flatIdentifier.formedFrom = FormedFrom.Ascending;
				result = FormedFrom.Ascending;
			}
			else
			{
				flatIdentifier.formedFrom = FormedFrom.Descending;
				result =  FormedFrom.Descending;
			}

			return result;
		}

		/// <summary>
		/// Функция находит ближайший экстремум, начиная поиск с левого края окна
		/// </summary>
		/// <param name="flatNumber">Номер объекта в списке боковиков</param>
		/// <returns>Свеча</returns>
		private _CandleStruct FindClosestExtremum(int flatNumber)
		{
			int candlesPassed = 1;
			FlatIdentifier currentFlat = flatCollection[flatNumber];

			// Цикл выполняется, пока на найдётся подходящий экстремум либо не пройдёт константное число итераций
			while (candlesPassed < _Constants.MaxFlatExtremumDistance)
			{
				candlesPassed++;
				int currentIndex = currentFlat.flatBounds.left.index - candlesPassed;
				_CandleStruct closestExtremum = globalCandles[currentIndex];

				if (globalCandles[currentIndex - 2].time == "10:00")
				{
					return globalCandles[0];
				}

				if (closestExtremum.low < currentFlat.gMin - _Constants.flatClassifyOffset * currentFlat.gMin &&
				    closestExtremum.low < globalCandles[currentIndex - 2].low &&
				    closestExtremum.low < globalCandles[currentIndex - 1].low &&
				    closestExtremum.low < globalCandles[currentIndex + 1].low &&
				    closestExtremum.low < globalCandles[currentIndex - 2].low)
				{
					return closestExtremum;
				}
				else if (closestExtremum.high > currentFlat.gMax + _Constants.flatClassifyOffset * currentFlat.gMax &&
				         closestExtremum.high > globalCandles[currentIndex - 2].high &&
				         closestExtremum.high > globalCandles[currentIndex - 1].high &&
				         closestExtremum.high > globalCandles[currentIndex + 1].high &&
				         closestExtremum.high > globalCandles[currentIndex - 2].high)
				{
					return closestExtremum;
				}
			}

			logger.Trace("Extremum haven't found");

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
		/// <param name="candleStructs"></param>
		/// <returns></returns>
		private double CalculateMeanOffsetDistance(List<FlatIdentifier> flatIdentifiers, List<_CandleStruct> candleStructs)
		{
			double meanDistance = 0;
			double distance = 0;
			int offsetsFound = 0;

			for (int i = 0; i < flatIdentifiers.Count - 1; i++)
			{
				for (int j = flatIdentifiers[i].flatBounds.right.index + 1; j < flatIdentifiers[i + 1].flatBounds.left.index; j++)
				{
					double currentOffset = Math.Abs(candleStructs[j].avg - flatIdentifiers[i].mean);
					if (currentOffset >= flatIdentifiers[i].flatWidth)
					{
						meanDistance += candleStructs[j].index - flatIdentifiers[i].flatBounds.right.index;
						offsetsFound++;
						logger.Trace("For flat {0} [{1} {2}] offset is at [{3}]: {4}", 
							flatIdentifiers[i].flatBounds.left.date,
							flatIdentifiers[i].flatBounds.left.time,
							flatIdentifiers[i].flatBounds.right.time,
							candleStructs[j].date, 
							candleStructs[j].time);
						break;
					}
				}
			}
			logger.Trace("[offsetsFound] = {0}", offsetsFound);

			meanDistance /= offsetsFound;
			return meanDistance;
		}
	}
}
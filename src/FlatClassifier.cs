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
		/// Сколько боковиков закрываются в падения
		/// </summary>
		private int flatsClosingToDescension;
		/// <summary>
		/// Сколько боковиков закрываются в взлёты
		/// </summary>
		private int flatsClosingToAscension;
		/// <summary>
		/// Средний интервал между боковиками
		/// </summary>
		private double meanFlatDuration;
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
				FormedFrom flatFormedFrom = ClassifyFormedFrom(flatCollection[i], i);
				
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

			for (int i = 0; i < flatsOverall - 1; i++)
			{
				ClosingTo flatClosingTo = ClassifyClosingTo(flatCollection[i] , i);
				switch (flatClosingTo)
				{
					case (ClosingTo.Ascension):
					{
						logger.Trace($"[{flatCollection[i].flatBounds.left.date}]: {flatCollection[i].flatBounds.right.time} closing to ascension");
						flatsClosingToAscension++;
						break;
					}
					case (ClosingTo.Descension):
					{
						logger.Trace($"[{flatCollection[i].flatBounds.left.date}]: {flatCollection[i].flatBounds.right.time} closing to descension");
						flatsClosingToDescension++;
						break;
					}
					default:
						break;
				}
			}

			int flatsFromAscensionPercantage 	= flatsFromAscension * 100 / flatsOverall;
			int flatsFromDescensionPercentage 	= flatsFromDescension * 100 / flatsOverall;
			int flatsToAscensionPercentage 		= flatsClosingToAscension * 100 / flatsOverall;
			int flatsToDescensionPercentage 	= flatsClosingToDescension * 100 / flatsOverall;

			logger.Trace($"From ascending = {flatsFromAscension} | From descending = {flatsFromDescension}");
			logger.Trace($"[fromAscending/fromDescending] = {flatsFromAscensionPercantage}%/{flatsFromDescensionPercentage}%");
			logger.Trace($"To ascending = {flatsClosingToAscension} | To descending = {flatsClosingToDescension}");
			logger.Trace($"[toAscending/toDescending] = {flatsToAscensionPercentage}%/{flatsToDescensionPercentage}%");

			meanFlatDuration = CalculateMeanFlatDuration(flatCollection);
			logger.Trace($"[meanFlatDuration] = {meanFlatDuration}");

			meanOffsetDistance = CalculateMeanOffsetDistance(flatCollection, globalCandles);
			logger.Trace($"[meanOffsetDistance] = {meanOffsetDistance}");
		}

		/// <summary>
		/// Определяет, что предшествовало боковому движению
		/// </summary>
		/// <param name="flat">Боковик</param>
		/// <param name="flatNumber">Номер боковика</param>
		/// <returns>Восходящий или нисходящий тренд</returns>
		private FormedFrom ClassifyFormedFrom(FlatIdentifier flat, int flatNumber)
		{
			_CandleStruct closestExtremum = FindClosestExtremum(flatNumber);
			FormedFrom result = closestExtremum.avg > flat.mean ? FormedFrom.Ascending : FormedFrom.Descending;
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

				if (closestExtremum.low < currentFlat.gMin - _Constants.FlatClassifyOffset * currentFlat.gMin &&
				    closestExtremum.low < globalCandles[currentIndex - 2].low &&
				    closestExtremum.low < globalCandles[currentIndex - 1].low &&
				    closestExtremum.low < globalCandles[currentIndex + 1].low &&
				    closestExtremum.low < globalCandles[currentIndex - 2].low)
				{
					return closestExtremum;
				}
				else if (closestExtremum.high > currentFlat.gMax + _Constants.FlatClassifyOffset * currentFlat.gMax &&
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
		private double CalculateMeanFlatDuration(List<FlatIdentifier> flatIdentifiers)
		{
			double result = 0;
			for (int i = 0; i < flatIdentifiers.Count; i++)
			{
				double currentDuration = flatIdentifiers[i].flatBounds.right.index - flatIdentifiers[i].flatBounds.left.index;
				result += currentDuration;
			}

			result /= flatIdentifiers.Count - 1;
			return result;
		}

		/// <summary>
		/// Функция вычисляет среднее расстояние между боковиками и их предстоящими отклонениями
		/// </summary>
		/// <param name="flats"></param>
		/// <param name="candleStructs"></param>
		/// <returns></returns>
		private double CalculateMeanOffsetDistance(List<FlatIdentifier> flats, List<_CandleStruct> candleStructs)
		{
			double meanDistance = 0;
			double distance = 0;
			int offsetsFound = 0;

			for (int i = 0; i < flats.Count - 1; i++)
			{
				for (int j = flats[i].flatBounds.right.index + 1; j < flats[i + 1].flatBounds.left.index; j++)
				{
					double currentOffset = Math.Abs(candleStructs[j].avg - flats[i].mean);
					if (currentOffset >= flats[i].flatWidth)
					{
						meanDistance += candleStructs[j].index - flats[i].flatBounds.right.index;
						offsetsFound++;
						logger.Trace($"For flat {flats[i].flatBounds.left.date} [{flats[i].flatBounds.left.time} {flats[i].flatBounds.right.time}] " +
						             $"offset is at [{candleStructs[j].date}]: {candleStructs[j].time}");
						break;
					}
				}
			}
			logger.Trace($"[offsetsFound] = {offsetsFound}");

			meanDistance /= offsetsFound;
			return meanDistance;
		}
		
		/// <summary>
		/// Определяет сторону закрытия боковика
		/// </summary>
		/// <param name="flat">Объект боковика</param>
		/// <param name="flatNumber">Номер объекта</param>
		/// <returns>Вниз или вверх</returns>
		private ClosingTo ClassifyClosingTo(FlatIdentifier flat, int flatNumber)
		{
			_CandleStruct closingCandle = FindClosingCandle(flatNumber);
			ClosingTo result = closingCandle.avg > flat.mean ? ClosingTo.Ascension : ClosingTo.Descension;
			return result;
		}

		/// <summary>
		/// Находит свечу, определяющую сторону закрытия боковика
		/// </summary>
		/// <param name="flatNumber">Номер боковика</param>
		/// <returns>Свеча снизу или сверху после движения</returns>
		private _CandleStruct FindClosingCandle(int flatNumber)
		{
			int currentIndex = flatCollection[flatNumber].flatBounds.right.index + 1;
			_CandleStruct result = globalCandles[currentIndex];
			double flatUpperBound = flatCollection[flatNumber].gMax;
			double flatLowerBound = flatCollection[flatNumber].gMin;
			double priceOffset = flatCollection[flatNumber].mean * _Constants.CloseCoeff;

			while (result.time != flatCollection[flatNumber + 1].flatBounds.left.time)
			{
				result = globalCandles[currentIndex];
				
				if (result.high > flatCollection[flatNumber].gMax + priceOffset ||
				    result.low  < flatCollection[flatNumber].gMin - priceOffset)
				{
					result =  globalCandles[currentIndex];
					logger.Trace($"Closing to candle: {result.time}");
					return result;
				}

				currentIndex++;
			}

			return result;
		}
	}
}
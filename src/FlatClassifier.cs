using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using NLog;

namespace FlatTraderBot
{
	public class FlatClassifier
	{
		private FlatClassifier()
		{
			logger.Trace("-----------------------------------------------------------------");
		}

		public FlatClassifier(IEnumerable<FlatIdentifier> flats, List<_CandleStruct> candles) : this()
		{
			flatList = new List<FlatIdentifier>(flats);
			globalCandles = candles;
			flatsOverall = flatList.Count;
		}

		/// <summary>
		/// Функция, запускающая анализ флетов
		/// </summary>
		public void ClassifyAllFlats()
		{
			for (int i = 0; i < flatsOverall; i++)
			{
				Direction flatFormedFrom = ClassifyFormedFrom(flatList[i], i);

				switch (flatFormedFrom)
				{
					case Direction.Up:
						logger.Trace(
							$"[{flatList[i].flatBounds.left.date}]: {flatList[i].flatBounds.left.time} from asceding");
						flatsFromAscension++;
						break;
					case Direction.Down:
						logger.Trace(
							$"[{flatList[i].flatBounds.left.date}]: {flatList[i].flatBounds.left.time} from descending");
						flatsFromDescension++;
						break;
					case Direction.Neutral:
						break;
					default:
						break;
				}
			}

			for (int i = 0; i < flatsOverall - 1; i++)
			{
				Direction flatClosingTo = ClassifyClosingTo(flatList[i] , i);
				switch (flatClosingTo)
				{
					case (Direction.Up):
					{
						logger.Trace($"[{flatList[i].flatBounds.left.date}]: {flatList[i].flatBounds.right.time} closing to ascension");
						flatsClosingToAscension++;
						break;
					}
					case (Direction.Down):
					{
						logger.Trace($"[{flatList[i].flatBounds.left.date}]: {flatList[i].flatBounds.right.time} closing to descension");
						flatsClosingToDescension++;
						break;
					}
					case Direction.Neutral:
						break;
					default:
						break;
				}
			}

			int flatsFromAscensionPercantage 	= flatsFromAscension 		* 100 / flatsOverall;
			int flatsFromDescensionPercentage 	= flatsFromDescension 		* 100 / flatsOverall;
			int flatsToAscensionPercentage 		= flatsClosingToAscension 	* 100 / flatsOverall;
			int flatsToDescensionPercentage 	= flatsClosingToDescension 	* 100 / flatsOverall;

			logger.Trace($"From ascending = {flatsFromAscension} | From descending = {flatsFromDescension}");
			logger.Trace($"[fromAscending/fromDescending] = {flatsFromAscensionPercantage}%/{flatsFromDescensionPercentage}%");
			logger.Trace($"To ascending = {flatsClosingToAscension} | To descending = {flatsClosingToDescension}");
			logger.Trace($"[toAscending/toDescending] = {flatsToAscensionPercentage}%/{flatsToDescensionPercentage}%");

			meanFlatDuration = CalculateMeanFlatDuration(flatList);
			logger.Trace($"[meanFlatDuration] = {meanFlatDuration}");

			meanOffsetDistance = CalculateMeanOffsetDistance(flatList, globalCandles);
			logger.Trace($"[meanOffsetDistance] = {meanOffsetDistance}");
		}

		/// <summary>
		/// Определяет, что предшествовало боковому движению
		/// </summary>
		/// <param name="flat">Боковик</param>
		/// <param name="flatNumber">Номер боковика</param>
		/// <returns>Восходящий или нисходящий тренд</returns>
		private Direction ClassifyFormedFrom(FlatIdentifier flat, int flatNumber)
		{
			_CandleStruct closestExtremum = FindClosestExtremum(flatNumber);
			Direction result = closestExtremum.avg > flat.mean ? Direction.Up : Direction.Down;
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
			FlatIdentifier currentFlat = flatList[flatNumber];

			// Цикл выполняется, пока на найдётся подходящий экстремум либо не пройдёт константное число итераций
			while (candlesPassed < _Constants.MaxFlatExtremumDistance)
			{
				candlesPassed++;
				int currentIndex = currentFlat.flatBounds.left.index - candlesPassed;
				_CandleStruct closestExtremum = globalCandles[currentIndex];

				if (globalCandles[currentIndex - 2].time == "10:00")
					return globalCandles[0];

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
		private double CalculateMeanFlatDuration(IReadOnlyCollection<FlatIdentifier> flatIdentifiers)
		{
			double result = 0;
			foreach (FlatIdentifier flat in flatIdentifiers)
			{
				double currentDuration = flat.flatBounds.right.index - flat.flatBounds.left.index;
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
		private double CalculateMeanOffsetDistance(IReadOnlyList<FlatIdentifier> flats, IReadOnlyList<_CandleStruct> candleStructs)
		{
			double meanDistance = 0;
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
		private Direction ClassifyClosingTo(FlatIdentifier flat, int flatNumber)
		{
			_CandleStruct closingCandle = FindClosingCandle(flatNumber);
			flat.closingCandle = closingCandle;
			Direction result = closingCandle.avg > flat.mean ? Direction.Up : Direction.Down;
			flat.closingTo = result;
			return result;
		}

		/// <summary>
		/// Находит свечу, определяющую сторону закрытия боковика
		/// </summary>
		/// <param name="flatNumber">Номер боковика</param>
		/// <returns>Свеча снизу или сверху после движения</returns>
		private _CandleStruct FindClosingCandle(int flatNumber)
		{
			FlatIdentifier currentFlat = flatList[flatNumber];
			int currentIndex = currentFlat.flatBounds.right.index + 1;
			_CandleStruct result = globalCandles[currentIndex];
			double priceOffset = currentFlat.mean * _Constants.CloseCoeff;
			double flatUpperBound = currentFlat.gMax + priceOffset;
			double flatLowerBound = currentFlat.gMin - priceOffset;

			while (result.time != flatList[flatNumber + 1].flatBounds.left.time)
			{
				result = globalCandles[currentIndex];
				if (result.close > flatUpperBound || result.close < flatLowerBound)
				{
					result = globalCandles[currentIndex];
					logger.Trace($"Closing to candle of [{currentFlat.flatBounds.left.time} {currentFlat.flatBounds.right.time}]: {result.time}");
					return result;
				}
				currentIndex++;
			}
			return result;
		}
	}
}
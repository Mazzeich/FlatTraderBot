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

			result /= flatIdentifiers.Count;
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

		/// <summary>
		/// Функция инициализирует поиск дальних пробоев для каждого боковика
		/// </summary>
		public void FindBreakthroughs()
		{
			for (int i = 0; i < flatsOverall - 1; i++)
			{
				if (flatList[i].closingTo == Direction.Down)
				{
					FindLowerBreakthroughs(i);
				}
				else
				{
					FindUpperBreakthroughs(i);
				}
			}
		}

		/// <summary>
		/// Функция находит дальний пробой для закрывающегося сверху боковика
		/// </summary>
		/// <param name="flatNumber">Номер боковика в списке</param>
		private void FindUpperBreakthroughs(int flatNumber)
		{
			FlatIdentifier currentFlat = flatList[flatNumber];
			FlatIdentifier nextFlat = flatList[flatNumber + 1];
			_Breakthrough breakthrough;
			breakthrough.candle = currentFlat.closingCandle;
			breakthrough.distanceToClose = 0;
			breakthrough.deltaPriceBreakthroughToClose = 0;
			int candlesPassed = 0;
			_CandleStruct iterator = globalCandles[currentFlat.closingCandle.index];

			while (iterator.time != nextFlat.flatBounds.left.time)
			{
				iterator = globalCandles[currentFlat.closingCandle.index + candlesPassed];
				double deltaClose = iterator.high - currentFlat.closingCandle.close;

				if (deltaClose > breakthrough.deltaPriceBreakthroughToClose)
				{
					breakthrough.candle = iterator;
					breakthrough.distanceToClose = iterator.index - currentFlat.closingCandle.index;
					breakthrough.deltaPriceBreakthroughToClose = deltaClose;
				}
				candlesPassed++;
			}

			logger.Trace($"[{currentFlat.flatBounds.left.date}] {currentFlat.flatBounds.left.time} {currentFlat.flatBounds.right.time}  " +
			             $"Дальний верхний пробой в [{breakthrough.candle.time}|{breakthrough.distanceToClose}|{breakthrough.deltaPriceBreakthroughToClose}]");
		}
		
		/// <summary>
		/// Функция находит дальний пробой для закрывающегося снизу боковика
		/// </summary>
		/// <param name="flatNumber">Номер боковика в списке</param>
		private void FindLowerBreakthroughs(int flatNumber)
		{
			FlatIdentifier currentFlat = flatList[flatNumber];
			FlatIdentifier nextFlat = flatList[flatNumber + 1];
			_Breakthrough breakthrough;
			breakthrough.candle = currentFlat.closingCandle;
			breakthrough.distanceToClose = 0;
			breakthrough.deltaPriceBreakthroughToClose = 0;
			int candlesPassed = 0;
			_CandleStruct iterator = globalCandles[currentFlat.closingCandle.index];

			while (iterator.time != nextFlat.flatBounds.left.time)
			{
				iterator = globalCandles[currentFlat.closingCandle.index + candlesPassed];
				double deltaClose = iterator.low - currentFlat.closingCandle.close;

				if (deltaClose < breakthrough.deltaPriceBreakthroughToClose)
				{
					breakthrough.candle = iterator;
					breakthrough.distanceToClose = iterator.index - currentFlat.closingCandle.index;
					breakthrough.deltaPriceBreakthroughToClose = deltaClose;
					flatList[flatNumber].breakthrough = breakthrough;
				}
				candlesPassed++;
			}
			
			logger.Trace($"[{currentFlat.flatBounds.left.date}] {currentFlat.flatBounds.left.time} {currentFlat.flatBounds.right.time}  " +
				             $"Дальний нижний пробой в [{breakthrough.candle.time}|{breakthrough.distanceToClose}|{breakthrough.deltaPriceBreakthroughToClose}]");
		}
		
		/// <summary>
		/// Логгер
		/// </summary>
		private readonly Logger logger = LogManager.GetCurrentClassLogger();
		/// <summary>
		/// Список всех найденных боковиков
		/// </summary>
		public  List<FlatIdentifier> flatList { get; set; }
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
	}
}
using System;
using System.Collections.Generic;
using FlatTraderBot.Structs;
using NLog;

namespace FlatTraderBot
{
	public class FlatClassifier
	{
		private FlatClassifier()
		{
			logger.Trace("-----------------------------------------------------------------");
		}

		public FlatClassifier(List<_CandleStruct> candles, ref List <FlatIdentifier> flats) : this()
		{
			flatList = new List<FlatIdentifier>(flats);
			globalCandles = candles;
			flatsOverall = flatList.Count;
		}

		/// <summary>
		/// Функция, запускающая классификацию флетов на сформированных с нисхождения либо с восхождения и закрывающихся аналогично
		/// </summary>
		public void ClassifyAllFlats()
		{
			logger.Trace("Classifying flats...");
			for (int i = 0; i < flatsOverall; i++)
			{
				Direction flatFormedFromDirection = ClassifyFormedFrom(flatList[i], i);
				Direction flatLeavingDirection  = ClassifyLeavingDirection(flatList[i] , i);

				switch (flatFormedFromDirection)
				{
					case Direction.Up:
						logger.Trace($"[{flatList[i].bounds.left.date}]: {flatList[i].bounds.left.time} from asceding");
						flatsFromAscension++;
						break;
					case Direction.Down:
						logger.Trace($"[{flatList[i].bounds.left.date}]: {flatList[i].bounds.left.time} from descending");
						flatsFromDescension++;
						break;
					case Direction.Neutral:
						break;
					default:
						break;
				}
				
				switch (flatLeavingDirection)
				{
					case (Direction.Up):
					{
						logger.Trace($"[{flatList[i].bounds.left.date}]: {flatList[i].bounds.right.time} leaving ↑");
						flatsLeavingToAscension++;
						break;
					}
					case (Direction.Down):
					{
						logger.Trace($"[{flatList[i].bounds.left.date}]: {flatList[i].bounds.right.time} leaving ↓");
						flatsLeavingToDescension++;
						break;
					}
					case Direction.Neutral:
						break;
					default:
						break;
				}

				ClassifyFlatType(flatFormedFromDirection, flatLeavingDirection);
			}

			int flatsFromAscensionPercantage  = flatsFromAscension 		 * 100 / flatsOverall;
			int flatsFromDescensionPercentage = flatsFromDescension 	 * 100 / flatsOverall;
			int flatsToAscensionPercentage 	  = flatsLeavingToAscension  * 100 / flatsOverall;
			int flatsToDescensionPercentage   = flatsLeavingToDescension * 100 / flatsOverall;

			logger.Trace($"From ascending = {flatsFromAscension} | From descending = {flatsFromDescension}");
			logger.Trace($"[fromAscending/fromDescending] = {flatsFromAscensionPercantage}%/{flatsFromDescensionPercentage}%");
			logger.Trace($"To ascending = {flatsLeavingToAscension} | To descending = {flatsLeavingToDescension}");
			logger.Trace($"[toAscending/toDescending] = {flatsToAscensionPercentage}%/{flatsToDescensionPercentage}%");

			meanFlatDuration = CalculateMeanFlatDuration(flatList);
			logger.Trace($"[meanFlatDuration] = {meanFlatDuration}");
			logger.Trace($"[type11] = {type11} [type10] = {type10} [type01] = {type01} [type00] = {type00}");
		}

		/// <summary> Определяет, что предшествовало боковому движению </summary>
		/// <param name="flat">Боковик</param>
		/// <param name="flatNumber">Номер боковика</param>
		/// <returns>Восходящий или нисходящий тренд</returns>
		private Direction ClassifyFormedFrom(FlatIdentifier flat, int flatNumber)
		{
			_CandleStruct closestExtremum = FindClosestExtremum(flatNumber);
			Direction result = closestExtremum.avg > flat.mean ? Direction.Up : Direction.Down;
			return result;
		}

		/// <summary> Функция находит ближайший экстремум слева, начиная поиск с левого края окна </summary>
		/// <param name="flatNumber">Номер объекта в списке боковиков</param>
		/// <returns>Свеча</returns>
		private _CandleStruct FindClosestExtremum(int flatNumber)
		{
			int candlesPassed = 1;
			FlatIdentifier currentFlat = flatList[flatNumber];

			// Цикл выполняется, пока на найдётся подходящий экстремум либо не пройдёт константное число итераций
			while (candlesPassed < _Constants.MaxFlatLeftExtremumDistance)
			{
				candlesPassed++;
				int currentIndex = currentFlat.bounds.left.index - candlesPassed;
				_CandleStruct closestExtremum = globalCandles[currentIndex];

				if (globalCandles[currentIndex].index < 5)
					return globalCandles[0];

				if (closestExtremum.low < currentFlat.gMin - _Constants.FlatClassifyOffset * currentFlat.gMin && IsCandleLowerThanNearests(closestExtremum))
				{
					return closestExtremum;
				}
				else if (closestExtremum.high > currentFlat.gMax + _Constants.FlatClassifyOffset * currentFlat.gMax && IsCandleHigherThanNearests(closestExtremum))
				{
					return closestExtremum;
				}
			}

			logger.Trace("Extremum not found");
			return globalCandles[0];
		}

		/// <summary> Функция возвращает true, если low свечи меньше low 4-х ближайших соседей </summary>
		/// <param name="candle">Свеча</param>
		/// <returns>Является ли свеча локальным минимумом</returns>
		private bool IsCandleLowerThanNearests(_CandleStruct candle)
		{
			int index = candle.index;
			double low = candle.low;
			return low < globalCandles[index - 2].low && low < globalCandles[index - 1].low &&
			       low < globalCandles[index + 1].low && low < globalCandles[index + 2].low;
		}
		
		/// <summary> Функция возвращает true, если high свечи больше  high 4-х ближайших соседей </summary>
		/// <param name="candle">Свеча</param>
		/// <returns>Является ли свеча локальным максимумом</returns>
		private bool IsCandleHigherThanNearests(_CandleStruct candle)
		{
			int index = candle.index;
			double high = candle.high;
			return high > globalCandles[index - 2].high && high > globalCandles[index - 1].high &&
			       high > globalCandles[index + 1].high && high > globalCandles[index + 2].high;
		}

		/// <summary> Функция вычисляет среднее расстояние между боковиками </summary>
		/// <param name="flatIdentifiers">Коллекция боковиков</param>
		/// <returns>Средний интервал</returns>
		private double CalculateMeanFlatDuration(IReadOnlyCollection<FlatIdentifier> flatIdentifiers)
		{
			double result = 0;
			foreach (FlatIdentifier flat in flatIdentifiers)
			{
				double currentDuration = flat.bounds.right.index - flat.bounds.left.index;
				result += currentDuration;
			}

			result /= flatIdentifiers.Count;
			return result;
		}

		/// <summary> Определяет направление выхода боковика </summary>
		/// <param name="flat">Объект боковика</param>
		/// <param name="flatNumber">Номер объекта</param>
		/// <returns>Вниз или вверх</returns>
		private Direction ClassifyLeavingDirection(FlatIdentifier flat, int flatNumber)
		{
			_CandleStruct leavingCandle = FindLeavingCandle(flatNumber);
			flat.leavingCandle = leavingCandle;
			logger.Trace($"Leaving to candle of [{flat.bounds.left.time} {flat.bounds.right.time}]: {leavingCandle.time}");
			if (leavingCandle.close > flat.mean)
			{
				const Direction result = Direction.Up;
				flat.leavingDirection = result;
				return result;
			}
			else if (leavingCandle.close < flat.mean)
			{
				const Direction result = Direction.Down;
				flat.leavingDirection = result;
				return result;
			}

			return Direction.Neutral;
		}

		/// <summary> Находит свечу, определяющую сторону выхода боковика </summary>
		/// <param name="flatNumber">Номер боковика</param>
		/// <returns>Свеча снизу или сверху после движения</returns>
		private _CandleStruct FindLeavingCandle(int flatNumber)
		{
			FlatIdentifier currentFlat = flatList[flatNumber];
			int currentIndex = currentFlat.bounds.right.index + 1;
			_CandleStruct result = globalCandles[currentIndex];
			double priceOffset = currentFlat.mean * _Constants.LeavingCoeff;
			double flatUpperBound = currentFlat.SDH + priceOffset;
			double flatLowerBound = currentFlat.SDL - priceOffset;

			if (flatNumber == flatsOverall - 1)
				return globalCandles[^1];
			
			while (result.time != flatList[flatNumber + 1].bounds.right.time)
			{
				result = globalCandles[currentIndex];
				if (result.high > flatUpperBound || result.low < flatLowerBound)
				{
					result = globalCandles[currentIndex];
					return result;
				}
				currentIndex++;
			}
			return result;
		}

		/// <summary>
		/// Классифицирует флет по типу направления формирования и направлению выхода из движения
		/// </summary>
		/// <param name="flatFormedFromDirection">Направление формирования флета</param>
		/// <param name="flatLeavingDirection">Направление выхода из флета</param>
		private void ClassifyFlatType(Direction flatFormedFromDirection, Direction flatLeavingDirection)
		{
			switch (flatFormedFromDirection)
			{
				case Direction.Up when flatLeavingDirection == Direction.Up:
					type11++;
					break;
				case Direction.Up when flatLeavingDirection == Direction.Down:
					type10++;
					break;
				case Direction.Down when flatLeavingDirection == Direction.Up:
					type01++;
					break;
				case Direction.Down when flatLeavingDirection == Direction.Down:
					type00++;
					break;
				case Direction.Neutral:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(flatFormedFromDirection), flatFormedFromDirection, null);
			}
		}

		/// <summary> Логгер </summary>
		private readonly Logger logger = LogManager.GetCurrentClassLogger();
		/// <summary> Список всех найденных боковиков </summary>
		private List<FlatIdentifier> flatList { get; set; }
		/// <summary> Глобальный список свечей </summary>
		private readonly List<_CandleStruct> globalCandles;
		/// <summary> Всего боковиков </summary>
		private readonly int flatsOverall;
		/// <summary> Сколько боковиков сформировано после падения </summary>
		private int flatsFromDescension;
		/// <summary> Сколько боковиков сформировано после взлёта </summary>
		private int flatsFromAscension;
		/// <summary> Сколько боковиков закрываются в падения </summary>
		private int flatsLeavingToDescension;
		/// <summary> Сколько боковиков закрываются в взлёты </summary>
		private int flatsLeavingToAscension;
		/// <summary> Средний интервал между боковиками </summary>
		private double meanFlatDuration;
		/// <summary>
		/// Флет сформирован сверху и уходит наверх
		/// </summary>
		private int type11;
		/// <summary>
		/// Флет сформирован сверху и уходит вниз
		/// </summary>
		private int type10;
		/// <summary>
		/// Флет сформирован снизу и уходи вверх
		/// </summary>
		private int type01;
		/// <summary>
		/// Флет сформирован снизу и уходит вниз
		/// </summary>
		private int type00;
	}
}
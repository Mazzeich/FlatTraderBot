using System;
using System.Collections.Generic;
using NLog;

namespace FlatTraderBot
{
	public class TakeProfitCandlesFinder
	{
		private TakeProfitCandlesFinder()
		{
			logger.Trace("-----------------------------------------------------------------");
		}

		public TakeProfitCandlesFinder(List<_CandleStruct> candles, ref List<FlatIdentifier> flatList) : this()
		{
			globalCandles = candles;
			this.flatList = flatList;
			flatsOverall = flatList.Count;
		}

		/// <summary>
		/// Функция инициализирует поиск дальних пробоев для каждого боковика
		/// </summary>
		public void FindTakeProfits()
		{
			logger.Trace("Finding take-profits...");
			for (int i = 0; i < flatsOverall - 1; i++)
			{
				FlatIdentifier currentFlat = flatList[i];
				FlatIdentifier nextFlat = flatList[i+1];

				switch (currentFlat.leavingDirection)
				{
					case Direction.Up:
						FindUpperTakeProfit(currentFlat, nextFlat);
						break;
					case Direction.Neutral:
						break;
					case Direction.Down:
						FindLowerTakeProfit(currentFlat, nextFlat);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			logger.Trace("-------------------------------------------------");
		}

		/// <summary>
		/// Функция ищет дальний пробой сверху для флета
		/// </summary>
		/// <param name="currentFlat">Рассматриваемый флет</param>
		/// <param name="nextFlat">Следующий за ним флет</param>
		private void FindUpperTakeProfit(FlatIdentifier currentFlat, FlatIdentifier nextFlat)
		{
			_TakeProfitCandle takeProfitCandle = InitializeTakeProfit(currentFlat);
			_CandleStruct iterator = globalCandles[currentFlat.leavingCandle.index];

			while (iterator.time != nextFlat.flatBounds.right.time)
			{
				iterator = globalCandles[iterator.index + 1];
				double deltaClose = iterator.high - currentFlat.leavingCandle.close;

				if (!(deltaClose > takeProfitCandle.deltaPriceTakeProfitToLeave))
				{
					continue;
				}

				takeProfitCandle.candle = iterator;
				takeProfitCandle.distanceToLeave = iterator.index - currentFlat.leavingCandle.index;
				takeProfitCandle.deltaPriceTakeProfitToLeave = deltaClose;
				currentFlat.takeProfitCandle = takeProfitCandle;
			}

			logger.Trace($"[{currentFlat.flatBounds.left.date}] {currentFlat.flatBounds.left.time} {currentFlat.flatBounds.right.time}  " +
			             $"Верхний тейк-профт в [{takeProfitCandle.candle.time} | {takeProfitCandle.distanceToLeave} свечей | {takeProfitCandle.deltaPriceTakeProfitToLeave}]");
		}
		
		/// <summary>
		/// Функция ищет дальний пробой снизу для флета
		/// </summary>
		/// <param name="currentFlat">Рассматриваемый флет</param>
		/// <param name="nextFlat">Следующий за ним флет</param>
		private void FindLowerTakeProfit(FlatIdentifier currentFlat, FlatIdentifier nextFlat)
		{
			_TakeProfitCandle takeProfitCandle = InitializeTakeProfit(currentFlat);
			int candlesPassed = 0;
			_CandleStruct iterator = globalCandles[currentFlat.leavingCandle.index];

			while (iterator.time != nextFlat.flatBounds.left.time)
			{
				iterator = globalCandles[currentFlat.leavingCandle.index + candlesPassed];
				double deltaClose = iterator.low - currentFlat.leavingCandle.close;

				if (deltaClose < takeProfitCandle.deltaPriceTakeProfitToLeave)
				{
					takeProfitCandle.candle = iterator;
					takeProfitCandle.distanceToLeave = iterator.index - currentFlat.leavingCandle.index;
					takeProfitCandle.deltaPriceTakeProfitToLeave = deltaClose;
					currentFlat.takeProfitCandle = takeProfitCandle;
				}
				candlesPassed++;
			}
			
			logger.Trace($"[{currentFlat.flatBounds.left.date}] {currentFlat.flatBounds.left.time} {currentFlat.flatBounds.right.time}  " +
			             $"Нижний тейк-профит в [{takeProfitCandle.candle.time} | {takeProfitCandle.distanceToLeave} свечей | {takeProfitCandle.deltaPriceTakeProfitToLeave}]");
		}
		
		/// <summary>
		/// Функция инициализирует дальний пробой
		/// </summary>
		/// <param name="flat">Флет</param>
		/// <returns>Структура дальнего пробоя</returns>
		private _TakeProfitCandle InitializeTakeProfit(FlatIdentifier flat)
		{
			_TakeProfitCandle b;
			b.candle = flat.leavingCandle;
			b.distanceToLeave = 0;
			b.deltaPriceTakeProfitToLeave = 0;
			return b;
		}
		
		/// <summary>
		/// Функция объединения пробоев
		/// </summary>
		public void RefreshTakeProfit()
		{
			logger.Trace("Uniting take-profits...");
			flatsOverall = flatList.Count;

			for (int i = 1; i < flatsOverall; i++)
			{
				FlatIdentifier currentFlat = flatList[i];
				FlatIdentifier previousFlat = flatList[i - 1];
				if (previousFlat.leavingDirection == currentFlat.leavingDirection)
				{
					logger.Trace($"[{previousFlat.flatBounds.left.date}] [{currentFlat.flatBounds.right.date}]");
					switch (currentFlat.leavingDirection)
					{
						case (Direction.Down):
						{
							RefreshLowerTakeProfit(i);
							logger.Trace($"Флеты {previousFlat.flatBounds.right.time} и {currentFlat.flatBounds.right.time}  " +
							             $"Нижний тейк-профит в [{previousFlat.takeProfitCandle.candle.time}|{currentFlat.takeProfitCandle.candle.time}]");
							
							break;
						}
						case (Direction.Up):
						{
							RefreshHigherTakeProfit(i);
							logger.Trace($"Флеты {previousFlat.flatBounds.right.time} и {currentFlat.flatBounds.right.time}  " +
							             $"Верхний тейк-профит в [{previousFlat.takeProfitCandle.candle.time}|{currentFlat.takeProfitCandle.candle.time}]");
							break;
						}
						case Direction.Neutral:
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}
		}

		private void RefreshLowerTakeProfit(int currentFlatNumber)
		{
			FlatIdentifier currentFlat = flatList[currentFlatNumber];
			FlatIdentifier previousFlat = flatList[currentFlatNumber - 1];
			if (currentFlat.takeProfitCandle.candle.close < previousFlat.takeProfitCandle.candle.close)
			{
				_TakeProfitCandle previousFlatTakeProfitCandle = previousFlat.takeProfitCandle;
				previousFlatTakeProfitCandle.candle.close = currentFlat.takeProfitCandle.candle.close;
				previousFlatTakeProfitCandle.distanceToLeave += currentFlat.candles.Count + currentFlat.takeProfitCandle.distanceToLeave;
				previousFlatTakeProfitCandle.deltaPriceTakeProfitToLeave = previousFlat.leavingCandle.close - currentFlat.takeProfitCandle.candle.close;
				previousFlat.takeProfitCandle = previousFlatTakeProfitCandle;
			}
		}
		private void RefreshHigherTakeProfit(int currentFlatNumber)
		{
			FlatIdentifier currentFlat = flatList[currentFlatNumber];
			FlatIdentifier previousFlat = flatList[currentFlatNumber - 1];
			if (currentFlat.takeProfitCandle.candle.close > previousFlat.takeProfitCandle.candle.close)
			{
				_TakeProfitCandle previousFlatTakeProfitCandle = previousFlat.takeProfitCandle;
				previousFlatTakeProfitCandle.candle.close = currentFlat.takeProfitCandle.candle.close;
				previousFlatTakeProfitCandle.distanceToLeave += currentFlat.candles.Count + currentFlat.takeProfitCandle.distanceToLeave;
				previousFlatTakeProfitCandle.deltaPriceTakeProfitToLeave = previousFlat.leavingCandle.close + currentFlat.takeProfitCandle.candle.close;
				previousFlat.takeProfitCandle = previousFlatTakeProfitCandle;
			}
		}

		/// <summary>
		/// Сбор статистики и усреднение тейк-профитов для примерного прогнозирования
		/// </summary>
		public void GetTakeProfitStatistics()
		{
			double meanTakeProfitDistanceToClose = 0;
			double meanTakeProfitDeltaPriceToClose = 0;
			for (int i = 0; i < flatsOverall - 1; i++)
			{
				meanTakeProfitDistanceToClose 	+= flatList[i].takeProfitCandle.distanceToLeave;
				meanTakeProfitDeltaPriceToClose += flatList[i].takeProfitCandle.deltaPriceTakeProfitToLeave;
			}

			meanTakeProfitDistanceToClose 	/= flatsOverall;
			meanTakeProfitDeltaPriceToClose /= flatsOverall;
			
			logger.Trace($"[meanTakeProfitDistanceToClose] = {meanTakeProfitDistanceToClose} [meanTakeProfitDeltaPriceToClose] = {meanTakeProfitDeltaPriceToClose}");
		}

		private readonly Logger logger = LogManager.GetCurrentClassLogger();
		private readonly List<_CandleStruct> globalCandles;
		private readonly List<FlatIdentifier> flatList;
		
		private int flatsOverall;
	}
}
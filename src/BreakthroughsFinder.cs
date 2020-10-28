using System;
using System.Collections.Generic;
using NLog;

namespace FlatTraderBot
{
	public class BreakthroughsFinder
	{
		private BreakthroughsFinder()
		{
			logger.Trace("-----------------------------------------------------------------");
		}

		public BreakthroughsFinder(List<_CandleStruct> candles, ref List<FlatIdentifier> flatList) : this()
		{
			globalCandles = candles;
			this.flatList = flatList;
			flatsOverall = flatList.Count;
		}

		/// <summary>
		/// Функция инициализирует поиск дальних пробоев для каждого боковика
		/// </summary>
		public void FindBreakthroughs()
		{
			logger.Trace("Finding breakthroughs...");
			for (int i = 0; i < flatsOverall - 1; i++)
			{
				FlatIdentifier currentFlat = flatList[i];
				FlatIdentifier nextFlat = flatList[i+1];

				switch (currentFlat.closingTo)
				{
					case Direction.Up:
						FindUpperBreakthrough(currentFlat, nextFlat);
						break;
					case Direction.Neutral:
						break;
					case Direction.Down:
						FindLowerBreakthrough(currentFlat, nextFlat);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		/// <summary>
		/// Функция ищет дальний пробой сверху для флета
		/// </summary>
		/// <param name="currentFlat">Рассматриваемый флет</param>
		/// <param name="nextFlat">Следующий за ним флет</param>
		private void FindUpperBreakthrough(FlatIdentifier currentFlat, FlatIdentifier nextFlat)
		{
			_Breakthrough breakthrough = AssignBreakthrough(currentFlat);
			_CandleStruct iterator = globalCandles[currentFlat.closingCandle.index];

			while (iterator.time != nextFlat.flatBounds.right.time)
			{
				iterator = globalCandles[iterator.index + 1];
				double deltaClose = iterator.high - currentFlat.closingCandle.close;

				if (!(deltaClose > breakthrough.deltaPriceBreakthroughToClose))
				{
					continue;
				}

				breakthrough.candle = iterator;
				breakthrough.distanceToClose = iterator.index - currentFlat.closingCandle.index;
				breakthrough.deltaPriceBreakthroughToClose = deltaClose;
				currentFlat.breakthrough = breakthrough;
			}

			logger.Trace($"[{currentFlat.flatBounds.left.date}] {currentFlat.flatBounds.left.time} {currentFlat.flatBounds.right.time}  " +
			             $"Дальний верхний пробой в [{breakthrough.candle.time} | {breakthrough.distanceToClose} свечей | {breakthrough.deltaPriceBreakthroughToClose}]");
		}
		
		/// <summary>
		/// Функция ищет дальний пробой снизу для флета
		/// </summary>
		/// <param name="currentFlat">Рассматриваемый флет</param>
		/// <param name="nextFlat">Следующий за ним флет</param>
		private void FindLowerBreakthrough(FlatIdentifier currentFlat, FlatIdentifier nextFlat)
		{
			_Breakthrough breakthrough = AssignBreakthrough(currentFlat);
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
					currentFlat.breakthrough = breakthrough;
				}
				candlesPassed++;
			}
			
			logger.Trace($"[{currentFlat.flatBounds.left.date}] {currentFlat.flatBounds.left.time} {currentFlat.flatBounds.right.time}  " +
			             $"Дальний нижний пробой в [{breakthrough.candle.time}|{breakthrough.distanceToClose}|{breakthrough.deltaPriceBreakthroughToClose}]");
		}
		
		/// <summary>
		/// Функция инициализирует дальний пробой
		/// </summary>
		/// <param name="flat">Флет</param>
		/// <returns>Структура дальнего пробоя</returns>
		private _Breakthrough AssignBreakthrough(FlatIdentifier flat)
		{
			_Breakthrough b;
			b.candle = flat.closingCandle;
			b.distanceToClose = 0;
			b.deltaPriceBreakthroughToClose = 0;
			return b;
		}
		
		public void UniteBreakthroughs()
		{
			logger.Trace("Uniting breakthroughs...");
			flatsOverall = flatList.Count;

			for (int i = 1; i < flatsOverall; i++)
			{
				FlatIdentifier currentFlat = flatList[i];
				FlatIdentifier previousFlat = flatList[i - 1];
				if (previousFlat.closingTo == currentFlat.closingTo)
				{
					logger.Trace($"[{previousFlat.flatBounds.left.date}] [{currentFlat.flatBounds.right.date}]");
					switch (currentFlat.closingTo)
					{
						case (Direction.Down):
						{
							UniteLowerBreakthroughs(i);
							logger.Trace($"Флеты {previousFlat.flatBounds.right.time} и {currentFlat.flatBounds.right.time}  " +
							             $"Дальние нижние пробои в [{previousFlat.breakthrough.candle.time}|{currentFlat.breakthrough.candle.time}]");
							
							break;
						}
						case (Direction.Up):
						{
							UniteHigherBreakthroughs(i);
							logger.Trace($"Флеты {previousFlat.flatBounds.right.time} и {currentFlat.flatBounds.right.time}  " +
							             $"Дальние верхние пробои в [{previousFlat.breakthrough.candle.time}|{currentFlat.breakthrough.candle.time}]");
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

		private void UniteLowerBreakthroughs(int currentFlatNumber)
		{
			FlatIdentifier currentFlat = flatList[currentFlatNumber];
			FlatIdentifier previousFlat = flatList[currentFlatNumber - 1];
			if (currentFlat.breakthrough.candle.close < previousFlat.breakthrough.candle.close)
			{
				_Breakthrough previousFlatBreakthrough = previousFlat.breakthrough;
				previousFlatBreakthrough.candle.close = currentFlat.breakthrough.candle.close;
				previousFlatBreakthrough.distanceToClose += currentFlat.candles.Count + currentFlat.breakthrough.distanceToClose;
				previousFlatBreakthrough.deltaPriceBreakthroughToClose = previousFlat.closingCandle.close - currentFlat.breakthrough.candle.close;
				previousFlat.breakthrough = previousFlatBreakthrough;
			}
		}
		private void UniteHigherBreakthroughs(int currentFlatNumber)
		{
			FlatIdentifier currentFlat = flatList[currentFlatNumber];
			FlatIdentifier previousFlat = flatList[currentFlatNumber - 1];
			if (currentFlat.breakthrough.candle.close > previousFlat.breakthrough.candle.close)
			{
				_Breakthrough previousFlatBreakthrough = previousFlat.breakthrough;
				previousFlatBreakthrough.candle.close = currentFlat.breakthrough.candle.close;
				previousFlatBreakthrough.distanceToClose += currentFlat.candles.Count + currentFlat.breakthrough.distanceToClose;
				previousFlatBreakthrough.deltaPriceBreakthroughToClose = previousFlat.closingCandle.close + currentFlat.breakthrough.candle.close;
				previousFlat.breakthrough = previousFlatBreakthrough;
			}
		}

		private Logger logger = LogManager.GetCurrentClassLogger();
		private List<_CandleStruct> globalCandles;
		private List<FlatIdentifier> flatList;
		private int flatsOverall;
	}
}
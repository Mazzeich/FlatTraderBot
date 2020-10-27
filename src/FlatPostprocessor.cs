using System;
using System.Collections.Generic;
using NLog;

namespace FlatTraderBot
{
	public class FlatPostprocessor
	{
		private FlatPostprocessor() {}
		
		public FlatPostprocessor(List<_CandleStruct> globalCandles, ref List<FlatIdentifier> flatList) : this()
		{
			this.globalCandles = globalCandles;
			this.flatList = flatList;
			flatsFound = flatList.Count;
		}

		/// <summary>
		/// Функция склеивает находящиеся близко друг к другу боковики
		/// </summary>
		public void UniteFlats()
		{
			for (int i = 1; i < flatsFound; i++)
            {
                FlatIdentifier currentFlat 	= flatList[i];
                FlatIdentifier prevFlat 	= flatList[i-1];
                
                bool areFlatsInTheSameDay 		= currentFlat.flatBounds.left.date == prevFlat.flatBounds.left.date;
                bool areFlatsTooClose 			= currentFlat.flatBounds.left.index - prevFlat.flatBounds.right.index <= _Constants.MinFlatGap;
                bool areFlatsMeansRoughlyEqual 	= Math.Abs(currentFlat.mean - prevFlat.mean) <= _Constants.FlatsMeanOffset * (currentFlat.mean + prevFlat.mean) * 0.5;
                bool isPrevFlatHasClosing		= prevFlat.flatBounds.left.time != currentFlat.closingCandle.time;

                logger.Trace($"{prevFlat.candles[0].date}: [{prevFlat.flatBounds.left.time} {prevFlat.flatBounds.right.time}] " +
                             $"and [{currentFlat.flatBounds.left.time} {currentFlat.flatBounds.right.time}] " +
                             $"Day = {areFlatsInTheSameDay} Distance = {areFlatsTooClose} Means = {areFlatsMeansRoughlyEqual} PrevFlatClosingAtCurrent = {isPrevFlatHasClosing}");

                // ЕСЛИ левая граница предыдущего и левая граница текущего находятся в пределах одного дня
                // И ЕСЛИ разница в свечах между левой границей текущего и правой границей предыдущего меьше ГАПА
                // И ЕСЛИ разница в цене между мат. ожиданиями текущего и предыдущего <= (ОФФСЕТ * среднее между мат. ожиданиями обоих боковиков)
                if (!areFlatsInTheSameDay || !areFlatsTooClose || !areFlatsMeansRoughlyEqual || !isPrevFlatHasClosing)
	                continue;

	            logger.Trace("Uniting");
                    
                List<_CandleStruct> newAperture = new List<_CandleStruct>(currentFlat.flatBounds.right.index - prevFlat.flatBounds.left.index);
                for (int j = prevFlat.flatBounds.left.index; j <= currentFlat.flatBounds.right.index; j++)
                {
	                newAperture.Add(globalCandles[j]);
                }
                FlatIdentifier newFlat = new FlatIdentifier();
                newFlat.AssignAperture(newAperture);
                newFlat.CalculateFlatProperties();
                newFlat.flatBounds = newFlat.SetBounds(newFlat.candles[0], newFlat.candles[^1]);
                newFlat.SetBounds(newFlat.candles[0], newFlat.candles[^1]);
                    
                flatList.RemoveRange(i-1, 2);
                flatList.Insert(i-1, newFlat);
                flatsFound--;
                i++;
                flatUnions++;
            }
		}

		public void UniteBreakthroughs()
		{
			logger.Trace("Uniting breakthroughs...");
			flatsFound = flatList.Count;

			for (int i = 1; i < flatsFound; i++)
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

		/// <summary>
		/// Инициализация логгера
		/// </summary>
		private readonly Logger logger = LogManager.GetCurrentClassLogger();
		/// <summary>
		/// Список найденных боковиков
		/// </summary>
		private List<FlatIdentifier> flatList { get; set; }
		/// <summary>
		/// Боковиков найдено
		/// </summary>
		private int flatsFound;
		/// <summary>
		/// Глобальный список свечей
		/// </summary>
		private readonly List<_CandleStruct> globalCandles;
		/// <summary>
		/// Количество операций объединения
		/// </summary>
		private int flatUnions;
	}
}
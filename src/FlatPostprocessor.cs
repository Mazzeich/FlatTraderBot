using System;
using System.Collections.Generic;
using NLog;

namespace FlatTraderBot
{
	public class FlatPostprocessor
	{
		private FlatPostprocessor() {}
		
		public FlatPostprocessor(HistoricalFlatFinder historicalFlatFinder) : this()
		{
			flatList = historicalFlatFinder.flatList;
			flatsFound = historicalFlatFinder.flatsFound;
			globalCandles = historicalFlatFinder.globalCandles;
		}

		/// <summary>
		/// Функция склеивает находящиеся близко друг к другу боковики
		/// </summary>
		public void UniteFlats()
		{
			for (int i = 1; i < flatsFound; i++)
            {
                FlatIdentifier currentFlat = flatList[i];
                FlatIdentifier prevFlat = flatList[i-1];
                
                bool areFlatsInTheSameDay 		= currentFlat.flatBounds.left.date == prevFlat.flatBounds.left.date;
                bool areFlatsTooClose 			= currentFlat.flatBounds.left.index - prevFlat.flatBounds.right.index <= _Constants.MinFlatGap;
                bool areFlatsMeansRoughlyEqual 	= Math.Abs(currentFlat.mean - prevFlat.mean) <= _Constants.flatsMeanOffset * (currentFlat.mean + prevFlat.mean) * 0.5;
                
                logger.Trace($"{prevFlat.candles[0].date}: [{prevFlat.flatBounds.left.time}] [{prevFlat.flatBounds.right.time}] " +
                             $"and [{currentFlat.flatBounds.left.time}] [{currentFlat.flatBounds.right.time}] " +
                             $"Day = {areFlatsInTheSameDay}\tDistance = {areFlatsTooClose}\tMeans = {areFlatsMeansRoughlyEqual}",
	                areFlatsInTheSameDay, areFlatsTooClose, areFlatsMeansRoughlyEqual);

                // ЕСЛИ левая граница предыдущего и левая граница текущего находятся в пределах одного дня
                // И ЕСЛИ разница в свечах между левой границей текущего и правой границей предыдущего меьше ГАПА
                // И ЕСЛИ разница в цене между мат. ожиданиями текущего и предыдущего <= (ОФФСЕТ * среднее между мат. ожиданиями обоих боковиков)
                if (areFlatsInTheSameDay && areFlatsTooClose && areFlatsMeansRoughlyEqual)
                {
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
                    unions++;
                }
                else
                {
	                continue;
                }
            }
		}

		private readonly Logger logger = LogManager.GetCurrentClassLogger();
		
		private List<FlatIdentifier> flatList = new List<FlatIdentifier>();

		private int flatsFound;

		private List<_CandleStruct> globalCandles = new List<_CandleStruct>();

		/// <summary>
		/// Количество операций объединения
		/// </summary>
		public int unions;
	}
}
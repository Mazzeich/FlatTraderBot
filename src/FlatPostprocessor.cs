using FlatTraderBot.Structs;
using NLog;
using System;
using System.Collections.Generic;

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
                FlatIdentifier currentFlat = flatList[i];
                FlatIdentifier prevFlat    = flatList[i-1];
                
                bool areFlatsInTheSameDay 	   = currentFlat.bounds.left.date == prevFlat.bounds.left.date;
                bool areFlatsTooClose 		   = currentFlat.bounds.left.index - prevFlat.bounds.right.index <= _Constants.MinFlatGap;
                bool areFlatsMeansRoughlyEqual = Math.Abs(currentFlat.mean - prevFlat.mean) <= _Constants.FlatsMeanOffset * (currentFlat.mean + prevFlat.mean) * 0.5;
                bool isPrevFlatHasClosing	   = prevFlat.bounds.left.time != currentFlat.leavingCandle.time;

                logger.Trace($"{prevFlat.candles[0].date}: [{prevFlat.bounds.left.time} {prevFlat.bounds.right.time}] " +
                             $"and [{currentFlat.bounds.left.time} {currentFlat.bounds.right.time}] " +
                             $"Day = {areFlatsInTheSameDay} Distance = {areFlatsTooClose} Means = {areFlatsMeansRoughlyEqual} PrevFlatCloseAtCurr = {isPrevFlatHasClosing}");

                // ЕСЛИ левая граница предыдущего и левая граница текущего находятся в пределах одного дня
                // И ЕСЛИ разница в свечах между левой границей текущего и правой границей предыдущего меьше ГАПА
                // И ЕСЛИ разница в цене между мат. ожиданиями текущего и предыдущего <= (ОФФСЕТ * среднее между мат. ожиданиями обоих боковиков)
                if (!areFlatsInTheSameDay || !areFlatsTooClose || !areFlatsMeansRoughlyEqual || !isPrevFlatHasClosing)
	                continue;

	            logger.Trace("Uniting");
                    
                List<_CandleStruct> newAperture = new List<_CandleStruct>(currentFlat.bounds.right.index - prevFlat.bounds.left.index);
                for (int j = prevFlat.bounds.left.index; j <= currentFlat.bounds.right.index; j++)
                {
	                newAperture.Add(globalCandles[j]);
                }
                FlatIdentifier newFlat = new FlatIdentifier();
                newFlat.AssignAperture(newAperture);
                newFlat.CalculateFlatProperties();
                newFlat.bounds = newFlat.SetBounds(newFlat.candles[0], newFlat.candles[^1]);
                newFlat.SetBounds(newFlat.candles[0], newFlat.candles[^1]);
                    
                flatList.RemoveRange(i-1, 2);
                flatList.Insert(i-1, newFlat);
                flatsFound--;
                i++;
                flatUnions++;
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
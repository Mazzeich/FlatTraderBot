using System.Collections.Generic;
using System;
using NLog;

namespace FlatTraderBot
{
	public class TakeProfitsFinder
	{
		private TakeProfitsFinder() { }

		public TakeProfitsFinder(List<_CandleStruct> globalCandles, ref List<FlatIdentifier> flatList) : this()
		{
			logger = LogManager.GetCurrentClassLogger();
			this.globalCandles = globalCandles;
			this.flatList = flatList;
			flatsOverall = this.flatList.Count;
		}

		public void FindAndSetTakeProfits()
		{
			for (int i = 0; i < flatsOverall;)
			{
				switch (flatList[i].leavingDirection)
				{
					case Direction.Up:
						SetTakeProfitsForUpLeavingFlats(ref i);
						break;
					case Direction.Neutral:
						break;
					case Direction.Down:
						SetTakeProfitsForDownLeavingFlats(ref i);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				if (i == flatsOverall - 1)
					break;
			}

			LogTakeProfits();
		}

		/// <summary> Находит и устанавливает тейк-профит для флета, закрывающегося вверх на логн </summary>
		/// <param name="i">Индекс флета</param>
		private void SetTakeProfitsForUpLeavingFlats(ref int i)
		{
			FlatIdentifier currentFlat = flatList[i];
			int flatsToOpposite = 0;
			currentFlat.takeProfitCandle.deltaPriceTakeProfitToLeave = double.NegativeInfinity;
			while (currentFlat.leavingDirection == flatList[i + flatsToOpposite].leavingDirection && i + flatsToOpposite < flatsOverall)
			{ 
				flatsToOpposite++;
			}

			FlatIdentifier nextOppositeFlat = flatList[i + flatsToOpposite];

			for (int j = currentFlat.leavingCandle.index; j < nextOppositeFlat.leavingCandle.index; j++)
			{
				if (globalCandles[j].high - currentFlat.leavingCandle.close > currentFlat.takeProfitCandle.deltaPriceTakeProfitToLeave)
				{
					_TakeProfitCandle takeProfit;
					takeProfit.candle = globalCandles[j];
					takeProfit.distanceToLeave = globalCandles[j].index - currentFlat.leavingCandle.index;
					takeProfit.deltaPriceTakeProfitToLeave = Math.Abs(globalCandles[j].high - currentFlat.leavingCandle.close);
					
					currentFlat.takeProfitCandle = takeProfit;
				}
			}

			flatList[i].takeProfitCandle = currentFlat.takeProfitCandle;
			i += flatsToOpposite;
		}

		/// <summary> Находит и устанавливает тейк-профит для флета, закрывающегося вниз на шорт </summary>
		/// <param name="i">Индекс флета</param>
		private void SetTakeProfitsForDownLeavingFlats(ref int i)
		{
			FlatIdentifier currentFlat = flatList[i];
			int flatsToOpposite = 0;
			currentFlat.takeProfitCandle.deltaPriceTakeProfitToLeave = double.PositiveInfinity;
			while (currentFlat.leavingDirection == flatList[i + flatsToOpposite].leavingDirection && i + flatsToOpposite < flatsOverall)
			{
				flatsToOpposite++;
			}

			for (int j = currentFlat.leavingCandle.index; j <= flatList[i + flatsToOpposite].leavingCandle.index; j++)
			{
				if (globalCandles[j].low - currentFlat.leavingCandle.close < currentFlat.takeProfitCandle.deltaPriceTakeProfitToLeave)
				{
					_TakeProfitCandle takeProfit;
					takeProfit.candle = globalCandles[j];
					takeProfit.distanceToLeave = globalCandles[j].index - currentFlat.leavingCandle.index;
					takeProfit.deltaPriceTakeProfitToLeave = Math.Abs(currentFlat.leavingCandle.close - globalCandles[j].low);

					currentFlat.takeProfitCandle = takeProfit;
				}
			}
			flatList[i].takeProfitCandle = currentFlat.takeProfitCandle;
			i += flatsToOpposite;
		}

		private void LogTakeProfits()
		{
			foreach (FlatIdentifier flat in flatList)
			{
				logger.Trace($"{flat.bounds.left.date} [{flat.bounds.left.time} {flat.bounds.right.time}] " +
				             $"{flat.takeProfitCandle.deltaPriceTakeProfitToLeave} {flat.takeProfitCandle.distanceToLeave}");
			}
		}

		private readonly Logger logger;
		private readonly List<_CandleStruct> globalCandles;
		private readonly List<FlatIdentifier> flatList;
		private readonly int flatsOverall;
	}
}
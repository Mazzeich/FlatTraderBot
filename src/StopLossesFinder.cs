using FlatTraderBot.Structs;
using NLog;
using System;
using System.Collections.Generic;

namespace FlatTraderBot
{
	public class StopLossesFinder
	{
		private StopLossesFinder() {}

		public StopLossesFinder(List<_CandleStruct> globalCandles, ref List<FlatIdentifier> flatList) : this()
		{
			this.globalCandles = globalCandles;
			this.flatList = flatList;
			flatsOverall = this.flatList.Count;
			logger = LogManager.GetCurrentClassLogger();
		}

		public void FindAndSetStopLosses()
		{
			foreach (FlatIdentifier flat in flatList)
			{
				switch (flat.leavingDirection)
				{
					case Direction.Down:
						// flat.stopLoss = flat.lowerBound - flat.mean * _Constants.LeavingCoeff; // 1
						// flat.stopLoss = flat.lowerBound;                                       // 2
						// flat.stopLoss = flat.upperBound + flat.mean * _Constants.LeavingCoeff; // 3
						// flat.stopLoss = flat.upperBound;                                       // 4
						flat.stopLoss = flat.mean;                                             // 5
						break;
					case Direction.Up:
						// flat.stopLoss = flat.upperBound + flat.mean * _Constants.LeavingCoeff; // 1
						// flat.stopLoss = flat.upperBound;                                       // 2
						// flat.stopLoss = flat.lowerBound - flat.mean * _Constants.LeavingCoeff; // 3
						// flat.stopLoss = flat.lowerBound;                                       // 4
						flat.stopLoss = flat.mean;                                             // 5
						break;
					case Direction.Neutral:
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			LogStopLossesInfo();
		}

		private void LogStopLossesInfo()
		{
			foreach (FlatIdentifier flat in flatList)
			{
				logger.Trace($"{flat.bounds.left.date} [{flat.bounds.left.time} {flat.bounds.right.time}]: {flat.stopLoss}");
			}
		}

		private readonly Logger logger;
		private List<_CandleStruct> globalCandles;
		private readonly List<FlatIdentifier> flatList;
		private int flatsOverall;
	}
}
using System;
using System.Collections.Generic;
using FlatTraderBot.Structs;
using NLog;

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
						flat.stopLoss = flat.upperBound + flat.mean * _Constants.LeavingCoeff;
						break;
					case Direction.Up:
						flat.stopLoss = flat.lowerBound - flat.mean * _Constants.LeavingCoeff;
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
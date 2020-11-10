using System;
using System.Collections.Generic;
using FlatTraderBot.Structs;
using NLog;

namespace FlatTraderBot
{
	public class Dealer
	{
		private Dealer() { }

		public Dealer(List<_CandleStruct> globalCandles, List<FlatIdentifier> flatList) : this()
		{
			this.globalCandles = globalCandles;
			this.flatList = flatList;
			globalCandlesOverall = this.globalCandles.Count;
			flatsOverall = this.flatList.Count;
			logger = LogManager.GetCurrentClassLogger();
			balanceAccount = _Constants.InitialBalance;
			dealsList = new List<_DealStruct>();
		}

		public void SimulateDealing()
		{
			_CandleStruct firstLeavingFlatCandle = FindFirstLeavingFlatCandle();
			int currentFlatIndex = 0;

			for (int i = firstLeavingFlatCandle.index; i < globalCandlesOverall;)
			{
				int flatsPassed = 0;
				int nextOppositeFlatIndex;
				switch (flatList[currentFlatIndex].leavingDirection)
				{
					case Direction.Up:
						nextOppositeFlatIndex = FindNextOppositeFlatIndex(currentFlatIndex);
						MakeLongDeal(currentFlatIndex, nextOppositeFlatIndex, ref i,  out flatsPassed);
						if (currentFlatIndex + flatsPassed == flatsOverall)
						{
							LogDealerInfo();
							return;
						}
						currentFlatIndex += flatsPassed;
						break;
					case Direction.Neutral:
						break;
					case Direction.Down:
						nextOppositeFlatIndex = FindNextOppositeFlatIndex(currentFlatIndex);
						MakeShortDeal(currentFlatIndex, nextOppositeFlatIndex, ref i, out flatsPassed);
						if (currentFlatIndex + flatsPassed == flatsOverall)
						{
							LogDealerInfo();
							return;
						}
						currentFlatIndex += flatsPassed;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			LogDealerInfo();
		}
		
		private _CandleStruct FindFirstLeavingFlatCandle()
		{
			for (int i = 0; i < globalCandlesOverall; i++)
			{
				if (globalCandles[i].date != flatList[0].leavingCandle.date ||
				    globalCandles[i].time != flatList[0].leavingCandle.time)
				{
					continue;
				}
				return globalCandles[i];
			}
			return default;
		}

		private _DealStruct InitializeDeal()
		{
			_DealStruct deal;
			deal.profit = default;
			deal.type = default;
			deal.OpenCandle = default;
			deal.CloseCandle = default;
			return deal;
		}

		private int FindNextOppositeFlatIndex(int currentFlatIndex)
		{
			for (int i = currentFlatIndex + 1; i < flatsOverall; i++)
			{
				if (flatList[currentFlatIndex].leavingDirection != flatList[i].leavingDirection)
					return i;
			}
			return 0;
		}

		private void MakeLongDeal(int currentFlatIndex, int nextOppositeFlatIndex, ref int i, out int flatsPassed)
		{
			flatsPassed = 0;
			int localIterator = 0;
			_DealStruct deal = default;
			FlatIdentifier currentFlat = flatList[currentFlatIndex];
			FlatIdentifier nextOppositeFlat = flatList[nextOppositeFlatIndex];
			double balanceBeforeDeal = balanceAccount;
			BuyOnPrice(currentFlat.leavingCandle.close);
			deal.type = 'L';
			deal.OpenCandle = currentFlat.leavingCandle;
			while (globalCandles[currentFlat.leavingCandle.index + localIterator].low > currentFlat.stopLoss &&
			       currentFlat.leavingCandle.index + localIterator < nextOppositeFlat.leavingCandle.index)
			{
				FlatIdentifier nextFlat = flatList[currentFlatIndex + flatsPassed + 1];
				if (currentFlat.leavingCandle.index + localIterator == nextFlat.leavingCandle.index)
				{
					// currentFlat.stopLoss = nextFlat.stopLoss;
					flatsPassed++;
				}
				localIterator++;
			}
			SellOnPrice(globalCandles[currentFlat.leavingCandle.index + localIterator].close);
			deal.profit = balanceAccount - balanceBeforeDeal;
			deal.CloseCandle = globalCandles[currentFlat.leavingCandle.index + localIterator];
			SetLeastAndMostProfitableDeals(deal);
			dealsList.Add(deal);
			i += localIterator;
			flatsPassed++;
		}
		
		private void MakeShortDeal(int currentFlatIndex, int nextOppositeFlatIndex, ref int i, out int flatsPassed)
		{
			flatsPassed = 0;
			int localIterator = 0;
			_DealStruct deal = default;
			FlatIdentifier currentFlat = flatList[currentFlatIndex];
			FlatIdentifier nextOppositeFlat = flatList[nextOppositeFlatIndex];
			double balanceBeforeDeal = balanceAccount;
			SellOnPrice(currentFlat.leavingCandle.close);
			deal.type = 'S';
			deal.OpenCandle = currentFlat.leavingCandle;
			while (globalCandles[currentFlat.leavingCandle.index + localIterator].high < currentFlat.stopLoss &&
			       currentFlat.leavingCandle.index + localIterator < nextOppositeFlat.leavingCandle.index)
			{
				FlatIdentifier nextFlat = flatList[currentFlatIndex + flatsPassed + 1];
				if (currentFlat.leavingCandle.index + localIterator == nextFlat.leavingCandle.index)
				{
					// currentFlat.stopLoss = nextFlat.stopLoss;
					flatsPassed++;
				}
				localIterator++;
			}
			BuyOnPrice(globalCandles[currentFlat.leavingCandle.index + localIterator].close);
			deal.profit = balanceAccount - balanceBeforeDeal;
			deal.CloseCandle = globalCandles[currentFlat.leavingCandle.index + localIterator];
			SetLeastAndMostProfitableDeals(deal);
			dealsList.Add(deal);
			i += localIterator;
			flatsPassed++;
		}
		
		private void SellOnPrice(double price)
		{
			try
			{
				balanceAccount += price * _Constants.NumberOfAssetsForDeal;
			}
			catch (Exception exception)
			{
				logger.Trace(exception);
				throw;
			}
		}

		private void BuyOnPrice(double price)
		{
			try
			{
				balanceAccount -= price * _Constants.NumberOfAssetsForDeal;
			}
			catch (Exception exception)
			{
				logger.Trace(exception);
				throw;
			}
		}
		
		private void SetLeastAndMostProfitableDeals(_DealStruct deal)
		{
			if (deal.profit > mostProfitDeal.profit)
			{
				mostProfitDeal = deal;
			}
			if (deal.profit < leastProfitDeal.profit)
			{
				leastProfitDeal = deal;
			}
		}

		private void LogDealerInfo()
		{
			CountProfitLossDeals();
			LogAllDeals();
			LogDealsConclusion();
		}
		
		private void CountProfitLossDeals()
		{
			foreach (_DealStruct deal in dealsList)
			{
				if (deal.profit > 0)
					profitDeals++;
				else
					lossDeals++;
			}
		}
		
		private void LogAllDeals()
		{
			foreach (_DealStruct d in dealsList)
			{
				logger.Trace($"[{d.OpenCandle.date}];[{d.OpenCandle.time}];[{d.CloseCandle.date}];[{d.CloseCandle.time}];{d.type};{d.profit}");
			}
		}
		
		private void LogDealsConclusion()
		{
			logger.Trace($"[DEALS];{profitDeals};{lossDeals}");
			logger.Trace($"Most profit;" +
			             $"[{mostProfitDeal.OpenCandle.date}];[{mostProfitDeal.OpenCandle.time}]" +
			             $";[{mostProfitDeal.CloseCandle.date}];[{mostProfitDeal.CloseCandle.time}];" +
			             $"{mostProfitDeal.profit};{mostProfitDeal.type}");
			logger.Trace($"Least profit;" +
			             $"[{leastProfitDeal.OpenCandle.date}];[{leastProfitDeal.OpenCandle.time}]" +
			             $";[{leastProfitDeal.CloseCandle.date}];[{leastProfitDeal.CloseCandle.time}];" +
			             $"{leastProfitDeal.profit};{mostProfitDeal.type}");
			logger.Trace($"[balance] = {balanceAccount} RUB");
		}

		private readonly Logger logger;
		private readonly List<_CandleStruct> globalCandles;
		private readonly List<FlatIdentifier> flatList;
		private readonly List<_DealStruct> dealsList;
		private readonly int globalCandlesOverall;
		private readonly int flatsOverall;
		private int profitDeals;
		private int lossDeals;
		private double balanceAccount;
		private _DealStruct mostProfitDeal;
		private _DealStruct leastProfitDeal;
	}
}
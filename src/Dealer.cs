using FlatTraderBot.Structs;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

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
						flatsPassed++;
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
						flatsPassed++;
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
		
		/// <summary> Функция находит свечу выхода из первого вреченного флета </summary>
		/// <returns>Объект свечи</returns>
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

		/// <summary> Функция находит индекс первого флета, закрывшегося в противоположную сторону от текущего </summary>
		/// <param name="currentFlatIndex">Индекс текущего флета</param>
		/// <returns>Индекс следующего закрывшегося в другую сторону флета</returns>
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
			bool isClosingLongDealTriggered = false;
			while (!isClosingLongDealTriggered && currentFlat.leavingCandle.index + localIterator < globalCandles[^1].index)
			{
				if (currentFlatIndex + flatsPassed == flatsOverall - 1)
				{
					break;
				}
				FlatIdentifier nextFlat = flatList[currentFlatIndex + flatsPassed + 1];
				if (currentFlat.leavingCandle.index + localIterator == nextFlat.leavingCandle.index)
				{
					currentFlat.stopLoss = nextFlat.stopLoss;
					flatsPassed++;
				}

				localIterator++;
				isClosingLongDealTriggered = IsClosingLongDealTriggered(globalCandles[currentFlat.leavingCandle.index + localIterator], currentFlat, nextOppositeFlat);
			}
			SellOnPrice(globalCandles[currentFlat.leavingCandle.index + localIterator].close);
			deal.profit = balanceAccount - balanceBeforeDeal;
			deal.CloseCandle = globalCandles[currentFlat.leavingCandle.index + localIterator];
			SetLeastAndMostProfitableDeals(deal);
			dealsList.Add(deal);
			i += localIterator;
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
			bool isClosingShortDealTriggered = false;
			while (!isClosingShortDealTriggered && currentFlat.leavingCandle.index + localIterator < globalCandles[^1].index)
			{
				if (currentFlatIndex + flatsPassed == flatsOverall - 1)
				{
					break;
				}
				FlatIdentifier nextFlat = flatList[currentFlatIndex + flatsPassed + 1];
				if (currentFlat.leavingCandle.index + localIterator == nextFlat.leavingCandle.index)
				{
					currentFlat.stopLoss = nextFlat.stopLoss;
					flatsPassed++;
				}

				localIterator++;
				isClosingShortDealTriggered = IsClosingShortDealTriggered(globalCandles[currentFlat.leavingCandle.index + localIterator], currentFlat, nextOppositeFlat);
			}
			BuyOnPrice(globalCandles[currentFlat.leavingCandle.index + localIterator].close);
			deal.profit = balanceAccount - balanceBeforeDeal;
			deal.CloseCandle = globalCandles[currentFlat.leavingCandle.index + localIterator];
			SetLeastAndMostProfitableDeals(deal);
			dealsList.Add(deal);
			i += localIterator;
		}

		private bool IsClosingShortDealTriggered(_CandleStruct currentCandle, FlatIdentifier currentFlat, FlatIdentifier nextOppositeFlat)
		{
			return IsShortStopLossTriggered(currentCandle, currentFlat) || IsShortTakeProfitTriggered(currentCandle, currentFlat) || IsDealExpired(currentCandle, currentFlat, nextOppositeFlat);
		}

		private bool IsClosingLongDealTriggered(_CandleStruct currentCandle, FlatIdentifier currentFlat, FlatIdentifier nextOppositeFlat)
		{
			return IsLongStopLossTriggered(currentCandle, currentFlat) || IsLongTakeProfitTriggered(currentCandle, currentFlat) || IsDealExpired(currentCandle, currentFlat, nextOppositeFlat);
		}

		private bool IsShortStopLossTriggered(_CandleStruct candle, FlatIdentifier flat)
		{
			return candle.high >= flat.stopLoss;
		}
		
		private bool IsLongStopLossTriggered(_CandleStruct candle, FlatIdentifier flat)
		{
			return candle.low <= flat.stopLoss;
		}

		private bool IsShortTakeProfitTriggered(_CandleStruct candle, FlatIdentifier flat)
		{
			return candle.low <= flat.leavingCandle.close - flat.mean * _Constants.TakeProfitPriceCoeff;
		}
		
		private bool IsLongTakeProfitTriggered(_CandleStruct candle, FlatIdentifier flat)
		{
			return candle.high >= flat.leavingCandle.close + flat.mean * _Constants.TakeProfitPriceCoeff;
		}

		private bool IsDealExpired(_CandleStruct currentCandle, FlatIdentifier currentFlat, FlatIdentifier nextOppositeFlat)
		{
			return currentCandle.index == nextOppositeFlat.leavingCandle.index;
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
				else if (deal.profit < 0)
					lossDeals++;
				else nonProfitDeals++;
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
			logger.Trace($"[DEALS];{profitDeals};{lossDeals};{nonProfitDeals}");
			logger.Trace($"Most profit;" +
			             $"[{mostProfitDeal.OpenCandle.date}];[{mostProfitDeal.OpenCandle.time}]" +
			             $";[{mostProfitDeal.CloseCandle.date}];[{mostProfitDeal.CloseCandle.time}];" +
			             $"{mostProfitDeal.profit};{mostProfitDeal.type}");
			logger.Trace($"Least profit;" +
			             $"[{leastProfitDeal.OpenCandle.date}];[{leastProfitDeal.OpenCandle.time}]" +
			             $";[{leastProfitDeal.CloseCandle.date}];[{leastProfitDeal.CloseCandle.time}];" +
			             $"{leastProfitDeal.profit};{mostProfitDeal.type}");
			logger.Trace($"[balance] = {balanceAccount} RUB");
			Console.WriteLine($"{profitDeals} {lossDeals} {balanceAccount} {leastProfitDeal.profit} {mostProfitDeal.profit}");
		}

		private readonly Logger logger;
		private readonly List<_CandleStruct> globalCandles;
		private readonly List<FlatIdentifier> flatList;
		private readonly List<_DealStruct> dealsList;
		private readonly int globalCandlesOverall;
		private readonly int flatsOverall;
		private int profitDeals;
		private int lossDeals;
		private int nonProfitDeals;
		private double balanceAccount;
		private _DealStruct mostProfitDeal;
		private _DealStruct leastProfitDeal;
	}
}
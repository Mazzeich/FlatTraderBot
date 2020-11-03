using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using NLog;

namespace FlatTraderBot
{
	public class BargainSimulation
	{
		private BargainSimulation() { }

		public BargainSimulation(List<_CandleStruct> candles, ref List<FlatIdentifier> flatList) : this()
		{
			globalCandles = candles;
			this.flatList = flatList;
			flatsOverall = flatList.Count;
			balanceAccount = 100000;
			profitDeals = 0;
			lossDeals = 0;
			mostProfitDeal.profit = double.NegativeInfinity;
			leastProfitDeal.profit = double.PositiveInfinity;
			dealsList = new List<_DealStruct>();
		}

		public void Start()
		{
			SetFlatStopLosses();
			logger.Trace($"\n[balance] = {balanceAccount}");
			for (int i = 0; i < flatsOverall - 1;)
			{
				Direction currentLeavingDirection = flatList[i].leavingDirection;
				switch (currentLeavingDirection)
				{
					case Direction.Down:
					{
						ShortDeal(ref i);
						break;
					}
					case Direction.Up:
					{
						LongDeal(ref i);
						break;
					}
					case Direction.Neutral:
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			CountProfitLossDeals();
			LogAllDeals();
			LogDealsConclusion();
		}

		private void SetFlatStopLosses()
		{
			foreach (FlatIdentifier flat in flatList)
			{
				if (flat.leavingCandle.close > flat.mean)
				{
					flat.stopLoss = flat.mean - flat.leavingCandle.close + flat.mean;
				} else if (flat.leavingCandle.close < flat.mean)
				{
					flat.stopLoss = flat.mean + flat.mean - flat.leavingCandle.close;
				}
			}
		}

		/// <summary>
		/// Совершение шорт-сделки
		/// </summary>
		/// <param name="currentFlatIndex">Индекс флета, после которого открывается сделка</param>
		private void ShortDeal(ref int currentFlatIndex)
		{
			const Direction openLeavingDirection = Direction.Down;
			_DealStruct deal = default;
			deal.type = 'S';
			deal.OpenCandle = flatList[currentFlatIndex].leavingCandle;
			double balanceBeforeDeal = balanceAccount;
			SellOnPrice(deal.OpenCandle.close);
			Direction newLeavingDirection = flatList[currentFlatIndex + 1].leavingDirection;
			if (newLeavingDirection == openLeavingDirection)
			{
				while (newLeavingDirection == openLeavingDirection && currentFlatIndex < flatsOverall - 2)
				{
					bool isStopLossTriggered = IsShortStopLossTriggered(ref currentFlatIndex, ref deal, balanceBeforeDeal);
					if (isStopLossTriggered)
						return;
					currentFlatIndex++;
					newLeavingDirection = flatList[currentFlatIndex + 1].leavingDirection;
				}
			} 
			else
			{
				bool isStopLossTriggered = IsShortStopLossTriggered(ref currentFlatIndex, ref deal, balanceBeforeDeal);
				if (isStopLossTriggered)
					return;
			}
			deal.CloseCandle = flatList[currentFlatIndex + 1].leavingCandle;
			BuyOnPrice(deal.CloseCandle.close);
			deal.profit = balanceAccount - balanceBeforeDeal;
			SetLeastAndMostProfitableDeals(deal);
			dealsList.Add(deal);
			currentFlatIndex++;
		}

		/// <summary>
		/// Совершение лонг-сделки
		/// </summary>
		/// <param name="currentFlatIndex">Индекс флета, после которого открывается сделка</param>
		private void LongDeal(ref int currentFlatIndex)
		{
			const Direction openLeavingDirection = Direction.Up;
			_DealStruct deal = default;
			deal.type = 'L';
			deal.OpenCandle = flatList[currentFlatIndex].leavingCandle;
			double balanceBeforeDeal = balanceAccount;
			BuyOnPrice(deal.OpenCandle.close);
			Direction newLeavingDirection = flatList[currentFlatIndex + 1].leavingDirection;
			if (newLeavingDirection == openLeavingDirection)
			{
				while (newLeavingDirection == openLeavingDirection && currentFlatIndex < flatsOverall - 2)
				{
					bool isStopLossTriggered = IsLongStopLossTriggered(ref currentFlatIndex, ref deal, balanceBeforeDeal);
					if (isStopLossTriggered)
						return;
					currentFlatIndex++;
					newLeavingDirection = flatList[currentFlatIndex + 1].leavingDirection;
				}
			}
			else
			{
				bool isStopLossTriggered = IsLongStopLossTriggered(ref currentFlatIndex, ref deal, balanceBeforeDeal);
				if (isStopLossTriggered)
					return;
			}
			deal.CloseCandle = flatList[currentFlatIndex + 1].leavingCandle;
			SellOnPrice(deal.CloseCandle.close);
			deal.profit = balanceAccount - balanceBeforeDeal;
			SetLeastAndMostProfitableDeals(deal);
			dealsList.Add(deal);
			currentFlatIndex++;
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

		private bool IsLongStopLossTriggered(ref int currentFlatIndex, ref _DealStruct deal, double balanceBeforeDeal)
		{
			FlatIdentifier currentFlat = flatList[currentFlatIndex];
			FlatIdentifier nextFlat = flatList[currentFlatIndex + 1];
			for (int i = currentFlat.leavingCandle.index; i < nextFlat.leavingCandle.index; i++)
			{
				if (globalCandles[i].low <= currentFlat.stopLoss)
				{
					deal.CloseCandle = globalCandles[i];
					SellOnPrice(deal.CloseCandle.close);
					deal.profit = balanceAccount - balanceBeforeDeal;
					SetLeastAndMostProfitableDeals(deal);
					dealsList.Add(deal);
					currentFlatIndex++;
					return true;
				}
			}
			return false;
		}

		private bool IsShortStopLossTriggered(ref int currentFlatIndex, ref _DealStruct deal, double balanceBeforeDeal)
		{
			FlatIdentifier currentFlat = flatList[currentFlatIndex];
			FlatIdentifier nextFlat = flatList[currentFlatIndex + 1];
			for (int i = currentFlat.leavingCandle.index; i < nextFlat.leavingCandle.index; i++)
			{
				if (globalCandles[i].high >= currentFlat.stopLoss)
				{
					deal.CloseCandle = globalCandles[i];
					BuyOnPrice(deal.CloseCandle.close);
					deal.profit = balanceAccount - balanceBeforeDeal;
					SetLeastAndMostProfitableDeals(deal);
					dealsList.Add(deal);
					currentFlatIndex++;
					return true;
				}
			}
			return false;
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
		
		private readonly Logger logger = LogManager.GetCurrentClassLogger();
		private readonly List<_CandleStruct> globalCandles;
		private readonly List<FlatIdentifier> flatList;
		private readonly List<_DealStruct> dealsList;
		private readonly int flatsOverall;
		private double balanceAccount;
		private int profitDeals;
		private int lossDeals;
		private _DealStruct mostProfitDeal;
		private _DealStruct leastProfitDeal;
	}
}
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
			logger.Trace($"\n[balance] = {balanceAccount}");
			for (int i = 0; i < flatsOverall - 2;)
			{
				Direction leavingDirection = flatList[i].leavingDirection;
				switch (leavingDirection)
				{
					case Direction.Down:
					{
						ShortDeal(ref i, leavingDirection);
						break;
					}
					case Direction.Up:
					{
						double balanceBeforeDeal = balanceAccount;
						LongDeal(ref i, leavingDirection);
						double deltaDeal = balanceAccount - balanceBeforeDeal;
						if (deltaDeal >= 0)
						{
							profitDeals++;
						}
						else
						{
							lossDeals++;
						}
						break;
					}
					case Direction.Neutral:
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			LogAllDeals();
			LogDealsConclusion();
		}

		private void ShortDeal(ref int currentFlatIndex, Direction openDirection)
		{
			_DealStruct deal = default;
			deal.type = 'S';
			deal.OpenCandle = flatList[currentFlatIndex].leavingCandle;
			double balanceBeforeDeal = balanceAccount;
			SellOnPrice(deal.OpenCandle.close);
			Direction newDirection = flatList[currentFlatIndex + 1].leavingDirection;
			while (newDirection == openDirection && currentFlatIndex + 1 < flatsOverall - 2)
			{
				currentFlatIndex++;
				newDirection = flatList[currentFlatIndex + 1].leavingDirection;
			}
			deal.CloseCandle = flatList[currentFlatIndex + 1].leavingCandle;
			BuyOnPrice(deal.CloseCandle.close);
			deal.profit = balanceAccount - balanceBeforeDeal;
			SetLeastAndMostProfitableDeals(deal);
			dealsList.Add(deal);
			currentFlatIndex++;
		}

		private void LongDeal(ref int currentFlatIndex, Direction openDirection)
		{
			_DealStruct deal = default;
			deal.type = 'L';
			deal.OpenCandle = flatList[currentFlatIndex].leavingCandle;
			double balanceBeforeDeal = balanceAccount;
			BuyOnPrice(deal.OpenCandle.close);
			Direction newDirection = flatList[currentFlatIndex + 1].leavingDirection;
			while (newDirection == openDirection && currentFlatIndex + 1 < flatsOverall - 2)
			{
				currentFlatIndex++;
				newDirection = flatList[currentFlatIndex + 1].leavingDirection;
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
				balanceAccount += price;
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
				balanceAccount -= price;
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
			             $"{mostProfitDeal.type};{mostProfitDeal.profit}");
			logger.Trace($"Least profit;" +
			             $"[{leastProfitDeal.OpenCandle.date}];[{leastProfitDeal.OpenCandle.time}]" +
			             $";[{leastProfitDeal.CloseCandle.date}];[{leastProfitDeal.CloseCandle.time}];" +
			             $"{mostProfitDeal.type};{leastProfitDeal.profit}");
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
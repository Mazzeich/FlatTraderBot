using FlatTraderBot.Structs;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML.Data;

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
			triggeredTakeProfits = 0;
			profitAccumulation = 0;
			lossAccumulation = 0;
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

		/// <summary> Проверка срабатывания условий для выхода из открытой сделки в шорте </summary>
		/// <param name="currentCandle">Текущая свеча</param>
		/// <param name="currentFlat">Флет, на которой была открыта сделка</param>
		/// <param name="nextOppositeFlat">Следующий флет, закрывшийся в противоположную сторону</param>
		/// <returns>Сработало ли одно из условий выхода из сделки</returns>
		private bool IsClosingShortDealTriggered(_CandleStruct currentCandle, FlatIdentifier currentFlat, FlatIdentifier nextOppositeFlat)
		{
			return IsShortStopLossTriggered(currentCandle, currentFlat) ||
			       IsShortTakeProfitTriggered(currentCandle, currentFlat) ||
			       IsDealExpired(currentCandle, nextOppositeFlat) ||
			       IsEndOfDay(currentCandle);
		}

		/// <summary> Проверка срабатывания условий для выхода из открытой сделки в лонге </summary>
		/// <param name="currentCandle">Текущая свеча</param>
		/// <param name="currentFlat">Флет, на которой была открыта сделка</param>
		/// <param name="nextOppositeFlat">Следующий флет, закрывшийся в противоположную сторону</param>
		/// <returns>Сработало ли одно из условий выхода из сделки</returns>
		private bool IsClosingLongDealTriggered(_CandleStruct currentCandle, FlatIdentifier currentFlat, FlatIdentifier nextOppositeFlat)
		{
			return IsLongStopLossTriggered(currentCandle, currentFlat) ||
			       IsLongTakeProfitTriggered(currentCandle, currentFlat) ||
			       IsDealExpired(currentCandle, nextOppositeFlat) ||
			       IsEndOfDay(currentCandle);
		}

		/// <summary> УСЛОВИЕ <br/> Сработал стоп-лосс по шорту
		/// </summary>
		/// <param name="candle">Текущая свеча</param>
		/// <param name="flat">Стоп-лосс флета</param>
		/// <returns>Сработало ли условие</returns>
		private bool IsShortStopLossTriggered(_CandleStruct candle, FlatIdentifier flat)
		{
			return candle.high >= flat.stopLoss;
		}
		
		/// <summary> УСЛОВИЕ <br/> Сработал стоп-лосс по лонгу
		/// </summary>
		/// <param name="candle">Текущая свеча</param>
		/// <param name="flat">Стоп-лосс флета</param>
		/// <returns>Сработало ли условие</returns>
		private bool IsLongStopLossTriggered(_CandleStruct candle, FlatIdentifier flat)
		{
			return candle.low <= flat.stopLoss;
		}

		/// <summary> УСЛОВИЕ <br/> Сработал тейк-профит по шорту </summary>
		/// <param name="candle">Текущая свеча</param>
		/// <param name="flat">Стоп-лосс флета</param>
		/// <returns>Сработало ли условие</returns>
		private bool IsShortTakeProfitTriggered(_CandleStruct candle, FlatIdentifier flat)
		{
			if (candle.low <= flat.leavingCandle.close - flat.mean * _Constants.TakeProfitPriceCoeff)
			{
				triggeredTakeProfits++;
				return true;
			}

			return false;
		}
		
		/// <summary> УСЛОВИЕ <br/> Сработал тейк-профит по шорту </summary>
		/// <param name="candle">Текущая свеча</param>
		/// <param name="flat">Стоп-лосс флета</param>
		/// <returns>Сработало ли условие</returns>
		private bool IsLongTakeProfitTriggered(_CandleStruct candle, FlatIdentifier flat)
		{
			if (candle.high >= flat.leavingCandle.close + flat.mean * _Constants.TakeProfitPriceCoeff)
			{
				triggeredTakeProfits++;
				return true;
			}

			return false;
		}

		/// <summary> УСЛОВИЕ <br/> Дошли до следующего противоположного флета </summary>
		/// <param name="currentCandle">Текущая свеча</param>
		/// <param name="currentFlat">Текущий флет</param>
		/// <param name="nextOppositeFlat">Следующий противоположный флет</param>
		/// <returns>Сработало ли условие</returns>
		private bool IsDealExpired(_CandleStruct currentCandle, FlatIdentifier nextOppositeFlat)
		{
			return currentCandle.index == nextOppositeFlat.leavingCandle.index;
		}

		/// <summary> УСЛОВИЕ <br/> Дошли до конца торгового дня </summary>
		/// <param name="currentCandle">Текущая свеча</param>
		/// <returns>Сработало ли условие</returns>
		private bool IsEndOfDay(_CandleStruct currentCandle)
		{
			// return false;
			return currentCandle.intradayIndex >= 101;
		}
		
		/// <summary> Совершить продажу по цене </summary>
		/// <param name="price">цена</param>
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

		/// <summary> Совершить покупку поцене </summary>
		/// <param name="price">Цена</param>
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
		
		/// <summary> Установка самой прибыльной и самой убыточной сделки </summary>
		/// <param name="deal">Объект сделки</param>
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

		/// <summary> Вызов функций логгирования по сделкам </summary>
		private void LogDealerInfo()
		{
			CountProfitLossDeals();
			LogAllDeals();
			LogDealsConclusion();
		}
		
		/// <summary> Подсчёт количества прибыльных, убыточных и нулевых сделок </summary>
		private void CountProfitLossDeals()
		{
			foreach (_DealStruct deal in dealsList)
			{
				if (deal.profit > 0)
				{
					profitDeals++;
					profitAccumulation += deal.profit;
				}
				else if (deal.profit < 0)
				{
					lossDeals++;
					lossAccumulation += deal.profit;
				}
				else nonProfitDeals++;
			}

			profitLossRatio = profitAccumulation / -lossAccumulation;
		}
		
		/// <summary> Логгирование информации по всем сделкам </summary>
		private void LogAllDeals()
		{
			List<_DealStruct> sortedDeals = dealsList.OrderByDescending(x => x.profit).ToList();
			foreach (_DealStruct d in sortedDeals)
			{
				logger.Trace($"[{d.OpenCandle.date}];[{d.OpenCandle.time}];[{d.CloseCandle.date}];[{d.CloseCandle.time}];{d.type};{d.profit}");
			}
		}
		
		/// <summary> Логгирование ключевых параметров статистики </summary>
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
			logger.Trace($"[triggered/not-triggered]: {triggeredTakeProfits}/{dealsList.Count}");
			logger.Trace($"[profitAccumulation] = {profitAccumulation} [lossAccumulation] = {lossAccumulation} [profitLossRatio] = {profitLossRatio}");
		}
		
		[ColumnName("Score")]
		public double balanceAccount;

		private readonly Logger logger;
		private readonly List<_CandleStruct> globalCandles;
		private readonly List<FlatIdentifier> flatList;
		private readonly List<_DealStruct> dealsList;
		private readonly int globalCandlesOverall;
		private readonly int flatsOverall;
		private double profitAccumulation;
		private double lossAccumulation;
		private double profitLossRatio;
		private int profitDeals;
		private int lossDeals;
		private int nonProfitDeals;
		private int triggeredTakeProfits;
		private _DealStruct mostProfitDeal;
		private _DealStruct leastProfitDeal;
	}
}
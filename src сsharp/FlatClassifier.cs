using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Candles
{
	public class FlatClassifier
	{
		/// <summary>
		/// Логгер
		/// </summary>
		private readonly Logger logger = LogManager.GetCurrentClassLogger();
		/// <summary>
		/// Список всех найденных боковиков
		/// </summary>
		private List<FlatIdentifier> flatCollection = new List<FlatIdentifier>();
		/// <summary>
		/// Глобальный список свечей
		/// </summary>
		private List<_CandleStruct> globalCandles = new List<_CandleStruct>();
		/// <summary>
		/// Всего боковиков
		/// </summary>
		private int flatsOverall;
		/// <summary>
		/// Сколько боковиков сформировано после падения
		/// </summary>
		private int flatsFromDescension;
		/// <summary>
		/// Сколько боковиков сформировано после взлёта
		/// </summary>
		private int flatsFromAscension;

		private FlatClassifier()
		{
			logger.Trace("[FlatClassifier] initialized");	
		}

		public FlatClassifier(List<FlatIdentifier> flats, List<_CandleStruct> candles) : this()
		{
			this.flatCollection = flats;
			globalCandles = candles;
			flatsOverall = flatCollection.Count;
		}

		public void ClassifyAllFlats()
		{
			logger.Trace("Classification started...");
			_CandleStruct closestMinimum = FindClosestMinimum();
			_CandleStruct closestMaximum = FindClosestMaximum();

			logger.Trace("Closest minimum is at {0}", closestMinimum.time);

		}

		private _CandleStruct FindClosestMinimum()
		{
			for (int globalIterator = flatCollection[1].flatBounds.leftBound.index; 
				globalIterator > flatCollection[1].flatBounds.leftBound.index - 10; globalIterator--)
			{
					
			}
			
			
		}

		private _CandleStruct FindClosestMaximum()
		{
			return new _CandleStruct();
		}
	}
}
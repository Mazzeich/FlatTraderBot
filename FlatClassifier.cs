using NLog;

namespace Candles
{
	public class FlatClassifier
	{
		/// <summary>
		/// Логгер
		/// </summary>
		private readonly Logger logger = LogManager.GetCurrentClassLogger();
		
		public FlatClassifier()
		{
		logger.Trace("[FlatClassifier] initialized");	
		}
	}
}
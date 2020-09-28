using System;
using NLog;
using NLog.Conditions;
using NLog.Config;
using NLog.Targets;
// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo

namespace Lua
{
    class Printer
    {
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private readonly FlatIdentifier fi;
        private readonly HistoricalFlatFinder hFF;

        public Printer()
        {
            // Подготовка логгера
            LoggingConfiguration config = new LoggingConfiguration();
            //FileTarget logfile = new FileTarget("Printer_logfile") { FileName = "loggerPrinter.txt" };
            ColoredConsoleTarget logconsole = new ColoredConsoleTarget("Printer_logconsole");
            ConsoleRowHighlightingRule highlightingRule = new ConsoleRowHighlightingRule
            {
                Condition = ConditionParser.ParseExpression("level == LogLevel.Trace"),
                ForegroundColor = ConsoleOutputColor.Green
            };
            logconsole.RowHighlightingRules.Add(highlightingRule);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            LogManager.Configuration = config;
        }
        public Printer(FlatIdentifier flatIdentifier) : this()
        {
            fi = flatIdentifier;
        }

        public Printer(HistoricalFlatFinder historicalFf) : this()
        {
            hFF = historicalFf;
        }

        public void OutputApertureInfo()
        {
            Console.WriteLine("[gMin] = {0} [{1}]\t[gMax] = {2} [{3}]", fi.GMin, fi.IdxGmin + 1, fi.GMax, fi.IdxGmax + 1);
            Console.WriteLine("[k] = {0}", fi.K);
            Console.WriteLine("[Median] = {0}", fi.Median);
            Console.WriteLine("[candles.Count] = {0}", fi.candles.Count);
            Console.WriteLine("[SDL] = {0}\t\t[SDH] = {1}", fi.SDL, fi.SDH);
            Console.WriteLine("[Экстремумы рядом с СКО low] = {0}\t[Экстремумы рядом с СКО high] = {1}", fi.ExsNearSDL, fi.ExsNearSDH);
            Console.WriteLine("[Границы окна]: [{0}]\t[{1}]", fi.FlatBounds.left.date, fi.FlatBounds.right.date);
            
            switch (fi.trend)
            {
                case Trend.Down:
                {
                    Console.Write("[Ширина коридора] = {0}\t", fi.flatWidth);
                    Console.WriteLine("[Минимальная ширина коридора] = {0}", _Constants.MinWidthCoeff * fi.Median);
                    Console.WriteLine("Аппроксимирующая линия имеет сильный убывающий тренд");
                    break;
                }
                case Trend.Up:
                {
                    Console.Write("[Ширина коридора] = {0}\t", fi.flatWidth);
                    Console.WriteLine("[Минимальная ширина коридора] = {0}", _Constants.MinWidthCoeff * fi.Median);
                    Console.WriteLine("Аппроксимирующая линия имеет сильный возрастающий тренд");
                    break;
                }
                case Trend.Neutral:
                {
                    Console.Write("[Ширина коридора] = {0}\t", fi.flatWidth);
                    Console.WriteLine("[Минимальная ширина коридора] = {0}", _Constants.MinWidthCoeff * fi.Median);
                    Console.WriteLine("Аппроксимирующая линия почти горизонтальна. Тренд нейтральный");
                    if (fi.IsFlat)
                    {
                        Console.WriteLine("Цена, вероятно, формирует боковик...");
                    }
                    break;
                }
                default: 
                    break;
            }

            if ((fi.flatWidth) < (_Constants.MinWidthCoeff * fi.Median))
            {
                Console.WriteLine("Боковик слишком узок");
            }

            Console.WriteLine();
        }

        public void WhyIsNotFlat(_CandleStruct leftBound, _CandleStruct rightBound)
        {
            string reason = "";
            Logger.Info("Окно с {0} по {1}", leftBound.date, rightBound.date);
            Logger.Info("В окне не определено боковое движение.\nВозможные причины:");

            
            switch (fi.trend)
            {
                case Trend.Down:
                {
                    reason += "Нисходящий тренд. ";
                    if ((fi.flatWidth) < (_Constants.MinWidthCoeff * fi.Median))
                    {
                        reason += "Недостаточная ширина коридора. ";
                    }

                    if (fi.ExsNearSDL < 2)
                    {
                        reason += "Недостаточно вершин снизу возле СКО.  ";
                    } else if (fi.ExsNearSDH < 2)
                    {
                        reason += "Недостаточно вершин сверху возле СКО. ";
                    }
                    break;
                }
                case Trend.Up:
                {
                    reason += "Восходящий тренд. ";
                    if ((fi.flatWidth) < (_Constants.MinWidthCoeff * fi.Median))
                    {
                        reason += "Недостаточная ширина коридора. ";
                    }

                    if (fi.ExsNearSDL < 2)
                    {
                        reason += "Недостаточно вершин снизу возле СКО.  ";
                    } else if (fi.ExsNearSDH < 2)
                    {
                        reason += "Недостаточно вершин сверху возле СКО. ";
                    }
                    break;
                }
                case Trend.Neutral:
                {
                    if ((fi.flatWidth) < (_Constants.MinWidthCoeff * fi.Median))
                    {
                        reason += "Недостаточная ширина коридора. ";
                    }

                    if (fi.ExsNearSDL < 2)
                    {
                        reason += "Недостаточно вершин снизу возле СКО.  ";
                    } else if (fi.ExsNearSDH < 2)
                    {
                        reason += "Недостаточно вершин сверху возле СКО. ";
                    }
                    break;
                }
            }

            //Console.WriteLine(reason);
            Logger.Info(reason);
            Console.WriteLine();
        }

        public void OutputHistoricalInfo()
        {
            Logger.Info("Боковиков найдено: {0}", hFF.FlatsFound);
            Logger.Info("Боковики определены в: ");
            for (int i = 0; i < hFF.ApertureBounds.Count; i++)
            {
                Logger.Info("[{0}]\t[{1}]", hFF.ApertureBounds[i].left.date, hFF.ApertureBounds[i].right.date);
            }
        }
    }
}

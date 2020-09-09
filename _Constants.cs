using System;

/// <summary>
/// Структура постоянных величин
/// </summary>
public struct _Constants
{
    /// <value> Минимальная ширина коридора (коэфф. от цены инструмента)</value>
    public const double minWidthCoeff = 0.005;

    /// <value> Коэффициент для определения поведения тренда</value>
    public const double kOffset = 0.01;

    /// <value> Возможное отклонение экстремума от линии СКО</value>
    public const double SDOffset = 0.0006;
}
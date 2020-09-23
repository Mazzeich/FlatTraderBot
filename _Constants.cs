using System;

/// <summary>
/// Структура постоянных величин
/// </summary>
public struct _Constants
{
    /// <value> Максимальный размер массива </value>
    public const int NGlobal = 20000;
    
    /// <value> Размер окна </value>
    public const int NAperture = 120;
    
    /// <value> Минимальная ширина коридора (коэфф. от цены инструмента)</value>
    public const double MinWidthCoeff = 0.002;

    /// <value> Коэффициент для определения поведения тренда</value>
    public const double KOffset = 0.01;

    /// <value> Возможное отклонение экстремума от линии СКО</value>
    public const double SDOffset = 0.0006;

    /// <value> Количество фазовых свечей, которые не нужно учитывать при вычислении уголового коэффициента </value>
    public const double PhaseCandlesCoeff = 0.05;
}
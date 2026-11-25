using System;

namespace Polyclinic.Core.Services;

/// <summary>
/// Сервис расчета индивидуальной скидки пациента (Модуль 2).
/// Расчет скидки производится на основе количества посещений за последние 12 месяцев:
/// - до 3 визитов: 0%
/// - 4–7 визитов: 5%
/// - 8–15 визитов: 10%
/// - более 15 визитов: 15%
/// </summary>
public static class DiscountCalculator
{
    /// <summary>
    /// Рассчитывает процент индивидуальной скидки пациента.
    /// </summary>
    /// <param name="visitsCountLast12Months">Количество посещений за последние 12 месяцев.</param>
    /// <returns>Процент скидки (0, 5, 10 или 15).</returns>
    /// <exception cref="ArgumentOutOfRangeException">Выбрасывается, если количество визитов отрицательно.</exception>
    public static int CalculateDiscount(int visitsCountLast12Months)
    {
        if (visitsCountLast12Months < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(visitsCountLast12Months), 
                "Количество посещений не может быть отрицательным числом.");
        }

        if (visitsCountLast12Months <= 3)
        {
            return 0;
        }
        else if (visitsCountLast12Months >= 4 && visitsCountLast12Months <= 7)
        {
            return 5;
        }
        else if (visitsCountLast12Months >= 8 && visitsCountLast12Months <= 15)
        {
            return 10;
        }
        else
        {
            return 15;
        }
    }

    /// <summary>
    /// Рассчитывает итоговую стоимость услуги с учетом индивидуальной скидки.
    /// </summary>
    public static decimal CalculateFinalCost(decimal baseCost, int discountPercent)
    {
        if (baseCost < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(baseCost), "Базовая стоимость не может быть отрицательной.");
        }

        if (discountPercent < 0 || discountPercent > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(discountPercent), "Скидка должна находиться в диапазоне от 0 до 100%.");
        }

        var discountAmount = baseCost * (discountPercent / 100m);
        return Math.Round(baseCost - discountAmount, 2);
    }
}

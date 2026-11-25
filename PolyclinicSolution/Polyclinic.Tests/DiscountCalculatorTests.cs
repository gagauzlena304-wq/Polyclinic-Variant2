using System;
using Polyclinic.Core.Services;
using Xunit;

namespace Polyclinic.Tests;

/// <summary>
/// Тестирование алгоритма расчета индивидуальной скидки (Модуль 2).
/// Градации:
/// до 3 визитов – 0%
/// 4–7 визитов – 5%
/// 8–15 визитов – 10%
/// более 15 визитов – 15%
/// </summary>
public class DiscountCalculatorTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(2, 0)]
    [InlineData(3, 0)]
    public void CalculateDiscount_UpTo3Visits_Returns0Percent(int visits, int expectedDiscount)
    {
        var result = DiscountCalculator.CalculateDiscount(visits);
        Assert.Equal(expectedDiscount, result);
    }

    [Theory]
    [InlineData(4, 5)]
    [InlineData(5, 5)]
    [InlineData(6, 5)]
    [InlineData(7, 5)]
    public void CalculateDiscount_4To7Visits_Returns5Percent(int visits, int expectedDiscount)
    {
        var result = DiscountCalculator.CalculateDiscount(visits);
        Assert.Equal(expectedDiscount, result);
    }

    [Theory]
    [InlineData(8, 10)]
    [InlineData(10, 10)]
    [InlineData(12, 10)]
    [InlineData(15, 10)]
    public void CalculateDiscount_8To15Visits_Returns10Percent(int visits, int expectedDiscount)
    {
        var result = DiscountCalculator.CalculateDiscount(visits);
        Assert.Equal(expectedDiscount, result);
    }

    [Theory]
    [InlineData(16, 15)]
    [InlineData(20, 15)]
    [InlineData(50, 15)]
    [InlineData(100, 15)]
    public void CalculateDiscount_MoreThan15Visits_Returns15Percent(int visits, int expectedDiscount)
    {
        var result = DiscountCalculator.CalculateDiscount(visits);
        Assert.Equal(expectedDiscount, result);
    }

    [Fact]
    public void CalculateDiscount_NegativeVisits_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DiscountCalculator.CalculateDiscount(-1));
    }

    [Theory]
    [InlineData(1500, 0, 1500)]
    [InlineData(2000, 5, 1900)]
    [InlineData(2000, 10, 1800)]
    [InlineData(2000, 15, 1700)]
    public void CalculateFinalCost_CalculatesCorrectly(decimal baseCost, int discount, decimal expectedFinal)
    {
        var finalCost = DiscountCalculator.CalculateFinalCost(baseCost, discount);
        Assert.Equal(expectedFinal, finalCost);
    }
}

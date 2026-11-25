using System.Collections.Generic;
using Polyclinic.Core.Services;
using Xunit;

namespace Polyclinic.Tests;

/// <summary>
/// Тестирование алгоритма расчета потребности в медикаментах (Модуль 4).
/// Формула:
/// material_per_unit = param1 * param2 * treatment_coefficient
/// total = material_per_unit * quantity * (1 + defect_rate)
/// return int(total)
/// При неверных данных/неизвестных типах -> -1.
/// </summary>
public class MedicineRequirementCalculatorTests
{
    [Fact]
    public void CalculateMedicineRequirement_ValidParameters_ReturnsCorrectInt()
    {
        // productTypeId = 2 (Инфузионная терапия, коэф = 1.10)
        // materialTypeId = 2 (Изотонический физраствор, defect_rate = 0.02)
        // quantity = 10 пациентов
        // param1 = 200 (мл за процедуру)
        // param2 = 5 (дней курса)
        // material_per_unit = 200 * 5 * 1.10 = 1100
        // total = 1100 * 10 * (1 + 0.02) = 11000 * 1.02 = 11220
        int result = MedicineRequirementCalculator.CalculateMedicineRequirement(
            productTypeId: 2,
            materialTypeId: 2,
            quantity: 10,
            param1: 200.0,
            param2: 5.0
        );

        Assert.Equal(11220, result);
    }

    [Fact]
    public void CalculateMedicineRequirement_AntibioticTherapy_ReturnsExpectedValue()
    {
        // productTypeId = 1 (Антибактериальная, коэф = 1.05)
        // materialTypeId = 1 (Антибиотик, defect_rate = 0.035)
        // quantity = 4 пациентов
        // param1 = 2.0 (флакона в день)
        // param2 = 7.0 (дней)
        // material_per_unit = 2.0 * 7.0 * 1.05 = 14.7
        // total = 14.7 * 4 * (1 + 0.035) = 58.8 * 1.035 = 60.858
        // (int)total = 60
        int result = MedicineRequirementCalculator.CalculateMedicineRequirement(
            productTypeId: 1,
            materialTypeId: 1,
            quantity: 4,
            param1: 2.0,
            param2: 7.0
        );

        Assert.Equal(60, result);
    }

    [Theory]
    [InlineData(999, 1, 10, 5.0, 5.0)] // Неизвестный product_type_id
    [InlineData(1, 999, 10, 5.0, 5.0)] // Неизвестный material_type_id
    [InlineData(1, 1, 0, 5.0, 5.0)]   // Нулевое количество пациентов
    [InlineData(1, 1, -5, 5.0, 5.0)]  // Отрицательное количество пациентов
    [InlineData(1, 1, 10, 0.0, 5.0)]  // Нулевой параметр 1
    [InlineData(1, 1, 10, -2.0, 5.0)] // Отрицательный параметр 1
    [InlineData(1, 1, 10, 5.0, 0.0)]  // Нулевой параметр 2
    [InlineData(1, 1, 10, 5.0, -1.0)] // Отрицательный параметр 2
    public void CalculateMedicineRequirement_InvalidInput_ReturnsMinusOne(
        int productTypeId, int materialTypeId, int quantity, double param1, double param2)
    {
        int result = MedicineRequirementCalculator.CalculateMedicineRequirement(
            productTypeId, materialTypeId, quantity, param1, param2);

        Assert.Equal(-1, result);
    }

    [Fact]
    public void CalculateMedicineRequirement_CustomDictionaries_CalculatesAccurately()
    {
        var customCoeffs = new Dictionary<int, double> { { 10, 1.25 } };
        var customDefects = new Dictionary<int, double> { { 20, 0.10 } };

        // param1=10, param2=2, coeff=1.25 -> 25
        // total = 25 * 2 * 1.10 = 55
        int result = MedicineRequirementCalculator.CalculateMedicineRequirement(
            productTypeId: 10,
            materialTypeId: 20,
            quantity: 2,
            param1: 10.0,
            param2: 2.0,
            customTreatmentCoefficients: customCoeffs,
            customDefectRates: customDefects
        );

        Assert.Equal(55, result);
    }
}

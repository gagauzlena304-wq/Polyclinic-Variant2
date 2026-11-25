using System;
using System.Collections.Generic;

namespace Polyclinic.Core.Services;

/// <summary>
/// Сервис расчета потребности в лекарственных препаратах (Модуль 4).
/// </summary>
public static class MedicineRequirementCalculator
{
    // Базовые коэффициенты типов лечения по умолчанию (product_type_id)
    public static readonly Dictionary<int, double> DefaultTreatmentCoefficients = new()
    {
        { 1, 1.05 }, // Антибактериальная терапия
        { 2, 1.10 }, // Инфузионная терапия (капельницы)
        { 3, 1.00 }, // Физиотерапия и ингаляции
        { 4, 1.08 }, // Инъекционная терапия
        { 5, 1.15 }  // Химиотерапия и таргетное лечение
    };

    // Базовые проценты потерь/брака препаратов по умолчанию (material_type_id)
    public static readonly Dictionary<int, double> DefaultDefectRates = new()
    {
        { 1, 0.035 }, // Антибиотик широкого спектра (3.5% брак)
        { 2, 0.020 }, // Изотонический физраствор (2.0% брак)
        { 3, 0.025 }, // Раствор глюкозы (2.5% брак)
        { 4, 0.040 }, // Обезболивающий раствор (4.0% брак)
        { 5, 0.015 }  // Витаминный комплекс (1.5% брак)
    };

    /// <summary>
    /// Метод расчета требуемого количества лекарства на курс лечения с учетом дозировки,
    /// продолжительности, количества пациентов и потерь (брака).
    /// </summary>
    /// <param name="productTypeId">Идентификатор типа лечения/назначения (product_type_id)</param>
    /// <param name="materialTypeId">Идентификатор типа препарата/лекарства (material_type_id)</param>
    /// <param name="quantity">Количество пациентов</param>
    /// <param name="param1">Параметр лечения 1 (например, дозировка в мл или дозах)</param>
    /// <param name="param2">Параметр лечения 2 (например, длительность курса в днях)</param>
    /// <param name="customTreatmentCoefficients">Пользовательский словарь коэффициентов лечения (опционально)</param>
    /// <param name="customDefectRates">Пользовательский словарь процентов брака (опционально)</param>
    /// <returns>Целое число единиц лекарства, либо -1 при неверных данных или неизвестных типах.</returns>
    public static int CalculateMedicineRequirement(
        int productTypeId, 
        int materialTypeId, 
        int quantity, 
        double param1, 
        double param2,
        IDictionary<int, double>? customTreatmentCoefficients = null,
        IDictionary<int, double>? customDefectRates = null)
    {
        // Проверка корректности числовых параметров
        if (quantity <= 0 || param1 <= 0 || param2 <= 0)
        {
            return -1;
        }

        if (double.IsNaN(param1) || double.IsInfinity(param1) ||
            double.IsNaN(param2) || double.IsInfinity(param2))
        {
            return -1;
        }

        var treatmentDict = customTreatmentCoefficients ?? DefaultTreatmentCoefficients;
        var defectDict = customDefectRates ?? DefaultDefectRates;

        // Поиск коэффициента типа лечения
        if (!treatmentDict.TryGetValue(productTypeId, out double treatmentCoefficient) || treatmentCoefficient <= 0)
        {
            return -1;
        }

        // Поиск процента брака для типа препарата
        if (!defectDict.TryGetValue(materialTypeId, out double defectRate) || defectRate < 0)
        {
            return -1;
        }

        // Формула по спецификации задания:
        // material_per_unit = param1 * param2 * treatment_coefficient
        // total = material_per_unit * quantity * (1 + defect_rate)
        // return int(total)
        double materialPerUnit = param1 * param2 * treatmentCoefficient;
        double total = materialPerUnit * quantity * (1.0 + defectRate);

        // Согласно формуле задания: return int(total)
        return (int)total;
    }
}

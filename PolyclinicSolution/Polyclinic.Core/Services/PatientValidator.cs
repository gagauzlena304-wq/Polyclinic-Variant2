using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Polyclinic.Core.Models;

namespace Polyclinic.Core.Services;

/// <summary>
/// Сервис валидации данных пациентов (Модуль 3).
/// </summary>
public static class PatientValidator
{
    private static readonly Regex PhoneRegex = new(@"^\+?[0-9\s\-\(\)]{10,20}$", RegexOptions.Compiled);
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex PolicyRegex = new(@"^\d{16}$", RegexOptions.Compiled);

    /// <summary>
    /// Выполняет полную валидацию модели пациента и возвращает список ошибок.
    /// </summary>
    public static List<string> Validate(Patient patient)
    {
        var errors = new List<string>();

        // ФИО
        if (string.IsNullOrWhiteSpace(patient.FullName))
        {
            errors.Add("Поле «ФИО» обязательно для заполнения.");
        }
        else
        {
            var parts = patient.FullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                errors.Add("ФИО должно содержать как минимум Фамилию и Имя.");
            }
        }

        // Дата рождения
        if (patient.BirthDate == default || patient.BirthDate > DateTime.Today)
        {
            errors.Add("Дата рождения не может быть пустой или находиться в будущем.");
        }
        else if (patient.BirthDate < DateTime.Today.AddYears(-125))
        {
            errors.Add("Указан некорректный год рождения (возраст более 125 лет).");
        }

        // Пол
        if (patient.GenderId <= 0)
        {
            errors.Add("Необходимо выбрать пол пациента из выпадающего списка.");
        }

        // Номер полиса ОМС
        var cleanPolicy = patient.PolicyNumber?.Replace(" ", "").Trim() ?? "";
        if (string.IsNullOrWhiteSpace(cleanPolicy))
        {
            errors.Add("Поле «Номер полиса ОМС» обязательно для заполнения.");
        }
        else if (!PolicyRegex.IsMatch(cleanPolicy))
        {
            errors.Add("Номер полиса ОМС должен состоять ровно из 16 цифр.");
        }

        // Адрес
        if (string.IsNullOrWhiteSpace(patient.Address))
        {
            errors.Add("Поле «Адрес проживания» обязательно для заполнения.");
        }

        // Телефон
        if (string.IsNullOrWhiteSpace(patient.Phone))
        {
            errors.Add("Поле «Номер телефона» обязательно для заполнения.");
        }
        else if (!PhoneRegex.IsMatch(patient.Phone.Trim()))
        {
            errors.Add("Неверный формат номера телефона. Ожидается: +7 (XXX) XXX-XX-XX.");
        }

        // Email
        if (string.IsNullOrWhiteSpace(patient.Email))
        {
            errors.Add("Поле «Email» обязательно для заполнения.");
        }
        else if (!EmailRegex.IsMatch(patient.Email.Trim()))
        {
            errors.Add("Неверный формат адреса электронной почты (пример: user@domain.ru).");
        }

        return errors;
    }

    public static string CleanPhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return string.Empty;
        var digits = Regex.Replace(phone, @"\D", "");
        if (digits.Length == 11 && (digits.StartsWith("7") || digits.StartsWith("8")))
        {
            return $"+7 ({digits.Substring(1, 3)}) {digits.Substring(4, 3)}-{digits.Substring(7, 2)}-{digits.Substring(9, 2)}";
        }
        return phone.Trim();
    }
}

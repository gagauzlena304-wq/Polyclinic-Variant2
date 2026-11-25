using System;
using Polyclinic.Core.Models;
using Polyclinic.Core.Services;
using Xunit;

namespace Polyclinic.Tests;

public class PatientValidatorTests
{
    [Fact]
    public void Validate_ValidPatient_ReturnsNoErrors()
    {
        var patient = new Patient
        {
            FullName = "Иванов Иван Иванович",
            BirthDate = new DateTime(1990, 5, 15),
            GenderId = 1,
            PolicyNumber = "1234567890123456",
            Address = "г. Москва, ул. Мира, д. 1",
            Phone = "+7 (999) 111-22-33",
            Email = "ivanov@test.ru"
        };

        var errors = PatientValidator.Validate(patient);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_InvalidPolicy_ReturnsError()
    {
        var patient = new Patient
        {
            FullName = "Петров Петр",
            BirthDate = new DateTime(1985, 1, 1),
            GenderId = 1,
            PolicyNumber = "12345", // Не 16 цифр
            Address = "г. Москва",
            Phone = "+79991234567",
            Email = "test@mail.ru"
        };

        var errors = PatientValidator.Validate(patient);
        Assert.Contains(errors, e => e.Contains("16 цифр"));
    }

    [Fact]
    public void Validate_InvalidEmail_ReturnsError()
    {
        var patient = new Patient
        {
            FullName = "Сидорова Анна",
            BirthDate = new DateTime(1995, 3, 20),
            GenderId = 2,
            PolicyNumber = "1111222233334444",
            Address = "г. Москва",
            Phone = "+7 (999) 000-00-00",
            Email = "invalid-email"
        };

        var errors = PatientValidator.Validate(patient);
        Assert.Contains(errors, e => e.Contains("Email") || e.Contains("почты"));
    }

    [Fact]
    public void Validate_FutureBirthDate_ReturnsError()
    {
        var patient = new Patient
        {
            FullName = "Будущий Пациент",
            BirthDate = DateTime.Today.AddDays(5),
            GenderId = 1,
            PolicyNumber = "9999888877776666",
            Address = "г. Москва",
            Phone = "+7 (999) 000-00-00",
            Email = "future@test.ru"
        };

        var errors = PatientValidator.Validate(patient);
        Assert.Contains(errors, e => e.Contains("будущем"));
    }
}

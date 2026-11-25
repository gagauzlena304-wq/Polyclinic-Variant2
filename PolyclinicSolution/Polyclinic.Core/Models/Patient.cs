using System;

namespace Polyclinic.Core.Models;

public class Patient
{
    public int PatientId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public int GenderId { get; set; }
    public string GenderName { get; set; } = string.Empty;
    public string PolicyNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Вычисляемые свойства для отображения в списке (Модуль 2 и 3)
    public int VisitsLast12Months { get; set; }
    public int CurrentDiscountPercent { get; set; }

    public int Age
    {
        get
        {
            var today = DateTime.Today;
            var age = today.Year - BirthDate.Year;
            if (BirthDate.Date > today.AddYears(-age)) age--;
            return Math.Max(0, age);
        }
    }

    public override string ToString() => $"{FullName} (Полис: {PolicyNumber})";
}

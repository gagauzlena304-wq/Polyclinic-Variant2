using System;

namespace Polyclinic.Core.Models;

public class Appointment
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string SpecializationName { get; set; } = string.Empty;
    public string Cabinet { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string Status { get; set; } = "Завершен";
    public string Complaints { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string Recommendations { get; set; } = string.Empty;
    public decimal BaseCost { get; set; }
    public int DiscountPercent { get; set; }
    public decimal FinalCost { get; set; }

    public string FormattedDate => AppointmentDate.ToString("dd.MM.yyyy HH:mm");
    public string FormattedBaseCost => $"{BaseCost:N2} ₽";
    public string FormattedFinalCost => $"{FinalCost:N2} ₽";
    public string FormattedDiscount => $"{DiscountPercent}%";
}

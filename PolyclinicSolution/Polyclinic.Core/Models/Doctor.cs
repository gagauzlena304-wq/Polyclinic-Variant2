namespace Polyclinic.Core.Models;

public class Doctor
{
    public int DoctorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int SpecializationId { get; set; }
    public string SpecializationName { get; set; } = string.Empty;
    public string Cabinet { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public override string ToString() => $"{FullName} ({SpecializationName}, каб. {Cabinet})";
}

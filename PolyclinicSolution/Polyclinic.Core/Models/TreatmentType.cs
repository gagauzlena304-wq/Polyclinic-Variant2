namespace Polyclinic.Core.Models;

public class TreatmentType
{
    public int TreatmentTypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public double TreatmentCoefficient { get; set; }

    public override string ToString() => $"{TypeName} (К={TreatmentCoefficient:F2})";
}

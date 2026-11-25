namespace Polyclinic.Core.Models;

public class Specialization
{
    public int SpecializationId { get; set; }
    public string SpecializationName { get; set; } = string.Empty;

    public override string ToString() => SpecializationName;
}

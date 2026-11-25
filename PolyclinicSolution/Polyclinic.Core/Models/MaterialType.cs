namespace Polyclinic.Core.Models;

public class MaterialType
{
    public int MaterialTypeId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public double DefectRate { get; set; }

    public override string ToString() => $"{MaterialName} (Брак={DefectRate:P1})";
}

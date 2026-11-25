namespace Polyclinic.Core.Models;

public class Gender
{
    public int GenderId { get; set; }
    public string GenderName { get; set; } = string.Empty;

    public override string ToString() => GenderName;
}

namespace Polyclinic.Core.Models;

public class Partner
{
    public int PartnerId { get; set; }
    public string PartnerName { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public override string ToString() => PartnerName;
}

using System;

namespace Polyclinic.Core.Models;

public class MedicineDelivery
{
    public int DeliveryId { get; set; }
    public int PartnerId { get; set; }
    public string PartnerName { get; set; } = string.Empty;
    public int MaterialTypeId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public string MedicineName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime DeliveryDate { get; set; }
    public string BatchNumber { get; set; } = string.Empty;

    public string FormattedDate => DeliveryDate.ToString("dd.MM.yyyy");
    public string FormattedQuantity => $"{Quantity:N0} шт.";
}

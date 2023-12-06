
using ECommerce.Domain.Commons;

namespace ECommerce.Domain.Entities;

public class Order : Auditable
{
    public long UserId { get; set; }
    public Users.User User { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveryDate { get; set; }
    public float TotalPrice { get; set; } = 0;
    public string Status { get; set; } = "Pending";

    public ICollection<OrderItem> OrderItems { get; set; }
}

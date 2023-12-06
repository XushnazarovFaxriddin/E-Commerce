using ECommerce.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Domain.Entities;

public class OrderItem : Auditable
{
    public long OrderId { get; set; }
    public Order Order { get; set; }
    public long MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; }
    public int Quantity { get; set; } = 1;
    public float Price { get; set; } = 0;
}

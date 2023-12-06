using ECommerce.Domain.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Domain.Entities
{
    public class MenuItem : Auditable
    {
        public Branch Branch { get; set; }
        public long BranchId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float Price { get; set; } = 0;
        public string Image { get; set; }
    }
}

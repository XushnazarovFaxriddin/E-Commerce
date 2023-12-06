using ECommerce.Domain.Commons;
using ECommerce.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Domain.Entities.Users;

public class UserComment : Auditable
{
    [Required, MaxLength(500)]
    public string Message { get; set; }
    public CommentType CommentType { get; set; }
    public long UserId { get; set; }
    public User User { get; set; }
}

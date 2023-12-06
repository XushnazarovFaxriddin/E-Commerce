using ECommerce.Domain.Commons;
using ECommerce.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Domain.Entities.Users;

public class User : Auditable
{
    [Length(3, 60)]
    public string FullName { get; set; }

    [Length(9,15), Phone]
    public string Phone { get; set; }

    public long TelegramId { get; set; }

    [Length(5, 60)]
    public string Username { get; set; }
    public LanguageType Language { get; set; }
    public UserRegisterStepType RegisterStep { get; set; }
    public CommentType? CommentType { get; set; }
    public UserStep? Step { get; set; }

    public ICollection<UserComment> UserComments { get; set; }
    public ICollection<Order> Orders { get; set; }
}

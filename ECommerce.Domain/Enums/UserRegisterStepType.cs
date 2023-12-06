using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Domain.Enums;

public enum UserRegisterStepType
{
    Start = 0,
    Language = 1,
    Phone = 2,
    FullName = 3,
    Success = 4
}

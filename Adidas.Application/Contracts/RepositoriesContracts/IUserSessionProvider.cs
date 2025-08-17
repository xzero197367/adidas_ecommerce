using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.Application.Contracts.RepositoriesContracts
{
    public interface IUserSessionProvider
    {
        string UserId { get; }
        string Email { get; }
        string Role { get; }

    }
}

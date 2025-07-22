using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concord.Application.Services.RefreshToken
{
    public interface IRefreshTokenService
    {
        Task<string> RefreshTokenAsync();
    }
}

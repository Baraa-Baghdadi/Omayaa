using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concord.Application.DTO.Identity
{
    public class LockAccountRequestDto
    {
        /// <summary>
        /// Lock until date (null to unlock immediately)
        /// </summary>
        public DateTimeOffset? LockUntil { get; set; }

        /// <summary>
        /// Reason for locking the account
        /// </summary>
        public string? Reason { get; set; }
    }
}

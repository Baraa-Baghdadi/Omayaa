using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concord.Application.DTO.Provider
{
    public class UpdateVerificationRequestDto
    {
        /// <summary>
        /// Email verification status
        /// </summary>
        public bool IsEmailVerified { get; set; }

        /// <summary>
        /// Phone verification status
        /// </summary>
        public bool IsPhoneVerified { get; set; }
    }
}

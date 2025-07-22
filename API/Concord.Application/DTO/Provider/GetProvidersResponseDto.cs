using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concord.Application.DTO.Provider
{
    public class GetProvidersResponseDto
    {
        /// <summary>
        /// List of providers
        /// </summary>
        public List<ProviderManagementDto> Providers { get; set; } = new List<ProviderManagementDto>();

        /// <summary>
        /// Pagination information
        /// </summary>
        public PaginationInfo Pagination { get; set; } = new PaginationInfo();
    }
}

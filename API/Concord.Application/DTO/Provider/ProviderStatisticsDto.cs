using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concord.Application.DTO.Provider
{
    public class ProviderStatisticsDto
    {
        /// <summary>
        /// Total number of providers
        /// </summary>
        public int TotalProviders { get; set; }

        /// <summary>
        /// Number of active providers
        /// </summary>
        public int ActiveProviders { get; set; }

        /// <summary>
        /// Number of inactive providers
        /// </summary>
        public int InactiveProviders { get; set; }

        /// <summary>
        /// Number of suspended providers
        /// </summary>
        public int SuspendedProviders { get; set; }

        /// <summary>
        /// Number of pending verification providers
        /// </summary>
        public int PendingVerificationProviders { get; set; }

        /// <summary>
        /// Providers registered this month
        /// </summary>
        public int RegisteredThisMonth { get; set; }

        /// <summary>
        /// Providers registered today
        /// </summary>
        public int RegisteredToday { get; set; }
    }
}

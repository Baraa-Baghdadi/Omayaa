namespace Concord.Application.DTO.Provider
{
    /// <summary>
    /// Simple DTO for provider dropdown lists
    /// Contains only essential information needed for selection controls
    /// </summary>
    public class ProviderDropdownDto
    {
        /// <summary>
        /// Provider unique identifier (Primary Key)
        /// </summary>
        public Guid ProviderId { get; set; }

        /// <summary>
        /// Provider business name for display
        /// </summary>
        public string ProviderName { get; set; }
    }
}

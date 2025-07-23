using System.ComponentModel.DataAnnotations;

namespace Concord.Application.DTO.Category
{
    public class CategoryDto
    {
        /// <summary>
        /// Category unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Category name
        /// </summary>
        [Required(ErrorMessage = "اسم الصنف مطلوب")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "اسم الصنف يجب أن يكون بين 2 و 100 حرف")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Number of products in this category
        /// </summary>
        public int ProductCount { get; set; }

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Last update date
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}

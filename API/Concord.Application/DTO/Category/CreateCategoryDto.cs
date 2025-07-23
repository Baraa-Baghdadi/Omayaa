using System.ComponentModel.DataAnnotations;

namespace Concord.Application.DTO.Category
{
    public class CreateCategoryDto
    {
        /// <summary>
        /// Category name
        /// </summary>
        [Required(ErrorMessage = "اسم الصنف مطلوب")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "اسم الصنف يجب أن يكون بين 2 و 100 حرف")]
        public string Name { get; set; } = string.Empty;
    }
}

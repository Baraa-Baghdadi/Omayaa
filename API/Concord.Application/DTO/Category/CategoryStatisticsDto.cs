namespace Concord.Application.DTO.Category
{
    public class CategoryStatisticsDto
    {
        /// <summary>
        /// Total number of categories
        /// </summary>
        public int TotalCategories { get; set; }

        /// <summary>
        /// Number of categories with products
        /// </summary>
        public int CategoriesWithProducts { get; set; }

        /// <summary>
        /// Number of empty categories
        /// </summary>
        public int EmptyCategories { get; set; }

        /// <summary>
        /// Categories created this month
        /// </summary>
        public int CreatedThisMonth { get; set; }

        /// <summary>
        /// Category with most products
        /// </summary>
        public string? TopCategoryName { get; set; }

        /// <summary>
        /// Number of products in top category
        /// </summary>
        public int TopCategoryProductCount { get; set; }

        /// <summary>
        /// Average products per category
        /// </summary>
        public double AverageProductsPerCategory { get; set; }
    }
}

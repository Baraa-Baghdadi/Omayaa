namespace Concord.Application.DTO.Product
{
    /// <summary>
    /// Product statistics for admin dashboard
    /// </summary>
    public class ProductStatisticsDto
    {
        /// <summary>
        /// Total number of products
        /// </summary>
        public int TotalProducts { get; set; }

        /// <summary>
        /// Number of active products
        /// </summary>
        public int ActiveProducts { get; set; }

        /// <summary>
        /// Number of inactive products
        /// </summary>
        public int InactiveProducts { get; set; }

        /// <summary>
        /// Number of products with discount (NewPrice)
        /// </summary>
        public int ProductsWithDiscount { get; set; }

        /// <summary>
        /// Number of products created this month
        /// </summary>
        public int CreatedThisMonth { get; set; }

        /// <summary>
        /// Average product price
        /// </summary>
        public decimal AveragePrice { get; set; }

        /// <summary>
        /// Most expensive product price
        /// </summary>
        public int HighestPrice { get; set; }

        /// <summary>
        /// Cheapest product price
        /// </summary>
        public int LowestPrice { get; set; }

        /// <summary>
        /// Category with most products
        /// </summary>
        public string? TopCategoryName { get; set; }

        /// <summary>
        /// Number of products in top category
        /// </summary>
        public int TopCategoryProductCount { get; set; }
    }
}

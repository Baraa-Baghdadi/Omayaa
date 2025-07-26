namespace Concord.Application.DTO.Product
{
    /// <summary>
    /// Product data transfer object for responses
    /// </summary>
    public class ProductDto
    {
        /// <summary>
        /// Product unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Product name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Regular price in the smallest currency unit
        /// </summary>
        public int Price { get; set; }

        /// <summary>
        /// Discounted price in the smallest currency unit (optional)
        /// </summary>
        public int? NewPrice { get; set; }

        /// <summary>
        /// Product image URL
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Whether the product is active and visible
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Category identifier this product belongs to
        /// </summary>
        public Guid CategoryId { get; set; }

        /// <summary>
        /// Category name for display purposes
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// Product creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}

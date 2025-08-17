using Concord.Application.DTO.Product;
using Concord.Application.DTO.Provider;
using Concord.Domain.Models.Categories;
using Concord.Domain.Models.Products;
using Concord.Domain.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;

namespace Concord.Application.Services.Products
{
    /// <summary>
    /// Service implementation for product management operations
    /// </summary>
    public class ProductManagementService : IProductManagementService
    {
        private readonly IGenericRepository<Product> _productRepository;
        private readonly IGenericRepository<Category> _categoryRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const string ProductImagesFolder = "images/products";

        public ProductManagementService(IGenericRepository<Product> productRepository, 
            IGenericRepository<Category> categoryRepository, 
            IWebHostEnvironment webHostEnvironment)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Gets all products with advanced filtering, sorting and pagination
        /// </summary>
        public async Task<GetProductsResponseDto> GetAllProductsAsync(GetProductsRequestDto request)
        {
            try
            {
                // Validate and set defaults for pagination
                var pageNumber = Math.Max(1, request.PageNumber);
                var pageSize = Math.Min(Math.Max(1, request.PageSize), 100);

                // Build filter expression
                Expression<Func<Product, bool>> filter = p => true;

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.Trim().ToLower();
                    filter = CombineWithAnd(filter, p => p.Name.ToLower().Contains(searchTerm));
                }

                // Apply category filter
                if (request.CategoryId.HasValue)
                {
                    filter = CombineWithAnd(filter, p => p.CategoryId == request.CategoryId.Value);
                }

                // Apply active status filter
                if (request.IsActive.HasValue)
                {
                    filter = CombineWithAnd(filter, p => p.IsActive == request.IsActive.Value);
                }

                // Apply price filters
                if (request.MinPrice.HasValue)
                {
                    filter = CombineWithAnd(filter, p => p.Price >= request.MinPrice.Value);
                }

                if (request.MaxPrice.HasValue)
                {
                    filter = CombineWithAnd(filter, p => p.Price <= request.MaxPrice.Value);
                }

                // Build sorting
                Func<IQueryable<Product>, IOrderedQueryable<Product>> orderBy = null;

                switch (request.SortBy?.ToLower())
                {
                    case "name":
                        orderBy = request.SortDirection?.ToLower() == "desc"
                            ? q => q.OrderByDescending(p => p.Name)
                            : q => q.OrderBy(p => p.Name);
                        break;
                    case "price":
                        orderBy = request.SortDirection?.ToLower() == "desc"
                            ? q => q.OrderByDescending(p => p.Price)
                            : q => q.OrderBy(p => p.Price);
                        break;
                    case "createdat":
                    default:
                        orderBy = request.SortDirection?.ToLower() == "asc"
                            ? q => q.OrderBy(p => p.CreatedAt)
                            : q => q.OrderByDescending(p => p.CreatedAt);
                        break;
                }

                // Include category information if requested
                var includeProperties = request.IncludeCategoryInfo ? "Category" : "";

                // Get paginated results
                var products = await _productRepository.GetAllWithFilterAsync(
                    filter,
                    orderBy,
                    includeProperties,
                    pageNumber,
                    pageSize
                );

                // Get total count for pagination
                var totalCount = await _productRepository.CountAsync(filter);

                // Map to DTOs
                var productDtos = products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    NewPrice = p.NewPrice,
                    ImageUrl = p.ImageUrl,
                    IsActive = p.IsActive,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category?.Name ?? "Unknown",
                    CreatedAt = p.CreatedAt
                }).ToList();

                // Create pagination info
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                var pagination = new PaginationInfo
                {
                    CurrentPage = pageNumber,
                    TotalPages = totalPages,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    SearchTerm = request.SearchTerm,
                    HasNextPage = pageNumber < totalPages,
                    HasPreviousPage = pageNumber > 1,
                    StartItem = (pageNumber - 1) * pageSize + 1,
                    EndItem = Math.Min(pageNumber * pageSize, totalCount)
                };

                return new GetProductsResponseDto
                {
                    Products = productDtos,
                    Pagination = pagination
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Gets all products for special category ID
        /// </summary>
        public async Task<List<ProductDto>> GetAllProductsAsync(Guid categoryId)
        {
            var products = await _productRepository.GetAllAsync("Category",x => x.CategoryId == categoryId);
            // Map to DTOs
            var productDtos = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                NewPrice = p.NewPrice,
                ImageUrl = p.ImageUrl,
                IsActive = p.IsActive,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name ?? "Unknown",
                CreatedAt = p.CreatedAt
            }).ToList();

            return productDtos;
        }

        /// <summary>
        /// Gets a specific product by ID
        /// </summary>
        public async Task<ProductDto?> GetProductByIdAsync(Guid productId)
        {
            try
            {
                var product = await _productRepository.GetFirstOrDefault(
                    p => p.Id == productId,
                    "Category"
                );

                if (product == null)
                {
                    return null;
                }

                return new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    NewPrice = product.NewPrice,
                    ImageUrl = product.ImageUrl,
                    IsActive = product.IsActive,
                    CategoryId = product.CategoryId,
                    CategoryName = product.Category?.Name ?? "Unknown",
                    CreatedAt = product.CreatedAt
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Creates a new product with optional image upload
        /// </summary>
        public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
        {
            try
            {

                // Validate category exists
                var categoryExists = await _categoryRepository.ExistsAsync(c => c.Id == createProductDto.CategoryId);
                if (!categoryExists)
                {
                    throw new ArgumentException("Category not found");
                }

                // Check if product name exists in the same category
                var nameExists = await ProductNameExistsInCategoryAsync(
                    createProductDto.Name,
                    createProductDto.CategoryId
                );
                if (nameExists)
                {
                    throw new ArgumentException("Product with this name already exists in the selected category");
                }

                // Handle image upload
                string imageUrl = "";
                if (createProductDto.Image != null)
                {
                    imageUrl = await SaveProductImageAsync(createProductDto.Image);
                }

                // Create product entity
                var product = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = createProductDto.Name.Trim(),
                    Price = createProductDto.Price,
                    NewPrice = createProductDto.NewPrice,
                    ImageUrl = imageUrl,
                    IsActive = createProductDto.IsActive,
                    CategoryId = createProductDto.CategoryId,
                    CreatedAt = DateTime.Now
                };

                // Save to database
                await _productRepository.AddAsync(product);
                await _productRepository.SaveChangesAsync();

                // Return created product
                return await GetProductByIdAsync(product.Id) ?? throw new InvalidOperationException("Failed to retrieve created product");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Updates an existing product with optional image upload
        /// </summary>
        public async Task<ProductDto?> UpdateProductAsync(Guid productId, UpdateProductDto updateProductDto)
        {
            try
            {
                // Get existing product
                var product = await _productRepository.GetByIdAsync(productId);
                if (product == null)
                {
                    return null;
                }

                // Validate category exists
                var categoryExists = await _categoryRepository.ExistsAsync(c => c.Id == updateProductDto.CategoryId);
                if (!categoryExists)
                {
                    throw new ArgumentException("Category not found");
                }

                // Check if product name exists in the same category (excluding current product)
                var nameExists = await ProductNameExistsInCategoryAsync(
                    updateProductDto.Name,
                    updateProductDto.CategoryId,
                    productId
                );
                if (nameExists)
                {
                    throw new ArgumentException("Product with this name already exists in the selected category");
                }

                // Handle image upload if new image provided
                if (updateProductDto.Image != null)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(product.ImageUrl))
                    {
                        await DeleteProductImageAsync(product.ImageUrl);
                    }

                    // Save new image
                    product.ImageUrl = await SaveProductImageAsync(updateProductDto.Image);
                }

                // Update product properties
                product.Name = updateProductDto.Name.Trim();
                product.Price = updateProductDto.Price;
                product.NewPrice = updateProductDto.NewPrice;
                product.IsActive = updateProductDto.IsActive;
                product.CategoryId = updateProductDto.CategoryId;

                // Save changes
                _productRepository.Update(product);
                await _productRepository.SaveChangesAsync();

                // Return updated product
                return await GetProductByIdAsync(productId);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Deletes a product and its associated image
        /// </summary>
        public async Task<bool> DeleteProductAsync(Guid productId)
        {
            try
            {

                var product = await _productRepository.GetByIdAsync(productId);
                if (product == null)
                {
                    return false;
                }

                // Delete associated image if exists
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    await DeleteProductImageAsync(product.ImageUrl);
                }

                // Delete product from database
                _productRepository.Delete(product);
                await _productRepository.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Gets product statistics for dashboard
        /// </summary>
        public async Task<ProductStatisticsDto> GetProductStatisticsAsync()
        {
            try
            {

                var allProducts = await _productRepository.GetAllAsync("Category");

                var totalProducts = allProducts.Count();
                var activeProducts = allProducts.Count(p => p.IsActive);
                var inactiveProducts = totalProducts - activeProducts;
                var productsWithDiscount = allProducts.Count(p => p.NewPrice.HasValue);

                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;
                var createdThisMonth = allProducts.Count(p =>
                    p.CreatedAt.Month == currentMonth && p.CreatedAt.Year == currentYear);

                var averagePrice = totalProducts > 0 ? allProducts.Average(p => p.Price) : 0;
                var highestPrice = totalProducts > 0 ? allProducts.Max(p => p.Price) : 0;
                var lowestPrice = totalProducts > 0 ? allProducts.Min(p => p.Price) : 0;

                // Get top category
                var categoryGroups = allProducts
                    .GroupBy(p => new { p.CategoryId, p.Category?.Name })
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();

                return new ProductStatisticsDto
                {
                    TotalProducts = totalProducts,
                    ActiveProducts = activeProducts,
                    InactiveProducts = inactiveProducts,
                    ProductsWithDiscount = productsWithDiscount,
                    CreatedThisMonth = createdThisMonth,
                    AveragePrice = (decimal)averagePrice,
                    HighestPrice = highestPrice,
                    LowestPrice = lowestPrice,
                    TopCategoryName = categoryGroups?.Key.Name,
                    TopCategoryProductCount = categoryGroups?.Count() ?? 0
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Checks if product name exists in the same category
        /// </summary>
        public async Task<bool> ProductNameExistsInCategoryAsync(string name, Guid categoryId, Guid? excludeId = null)
        {
            try
            {
                Expression<Func<Product, bool>> filter = p =>
                    p.Name.ToLower() == name.ToLower() && p.CategoryId == categoryId;

                if (excludeId.HasValue)
                {
                    filter = CombineWithAnd(filter, p => p.Id != excludeId.Value);
                }

                return await _productRepository.ExistsAsync(filter);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Combines two expressions with AND operator
        /// </summary>
        private Expression<Func<T, bool>> CombineWithAnd<T>(
            Expression<Func<T, bool>> first,
            Expression<Func<T, bool>> second)
        {
            var parameter = Expression.Parameter(typeof(T));
            var body = Expression.AndAlso(
                Expression.Invoke(first, parameter),
                Expression.Invoke(second, parameter)
            );
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        /// <summary>
        /// Saves uploaded image to wwwroot/images/products folder
        /// </summary>
        private async Task<string> SaveProductImageAsync(IFormFile image)
        {
            try
            {
                // Create images/products directory if it doesn't exist
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, ProductImagesFolder);
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique filename
                var fileExtension = Path.GetExtension(image.FileName);
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                // Return relative URL
                return $"/{ProductImagesFolder}/{fileName}".Replace('\\', '/');
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to save product image", ex);
            }
        }

        /// <summary>
        /// Deletes product image from wwwroot folder
        /// </summary>
        private async Task DeleteProductImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                    return;

                // Convert URL to physical path
                var fileName = Path.GetFileName(imageUrl);
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, ProductImagesFolder, fileName);

                // Delete file if exists
                if (File.Exists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                }
            }
            catch (Exception ex)
            {
                // Don't throw - image deletion failure shouldn't prevent product operations
            }
        }

        #endregion
    }
}

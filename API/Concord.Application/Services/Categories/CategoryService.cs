using Concord.Application.DTO.Category;
using Concord.Application.DTO.Provider;
using Concord.Domain.Models.Categories;
using Concord.Domain.Repositories;
using System.Linq.Expressions;

namespace Concord.Application.Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly IGenericRepository<Category> _categoryRepository;

        public CategoryService(IGenericRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        /// <summary>
        /// Gets all categories with advanced filtering, sorting and pagination
        /// </summary>
        public async Task<GetCategoriesResponseDto> GetAllCategoriesAsync(GetCategoriesRequestDto request)
        {
            try
            {
                // Build filter expression
                Expression<Func<Category, bool>>? filter = BuildFilterExpression(request);

                // Get total count for pagination
                var totalCount = await _categoryRepository.CountAsync(filter);

                // Calculate pagination values
                var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
                var hasNextPage = request.PageNumber < totalPages;
                var hasPreviousPage = request.PageNumber > 1;
                var startItem = (request.PageNumber - 1) * request.PageSize + 1;
                var endItem = Math.Min(request.PageNumber * request.PageSize, totalCount);

                // Build order expression
                Func<IQueryable<Category>, IOrderedQueryable<Category>> orderBy = BuildOrderExpression(request);

                // Include products if needed
                string includes = request.IncludeProductCount ? "Products" : "";

                // Get categories with pagination
                var categories = await _categoryRepository.GetAllWithFilterAsync(
                    filter: filter,
                    orderBy: orderBy,
                    includeProperties: includes,
                    pageNumber: request.PageNumber,
                    pageSize: request.PageSize
                );

                // Map to DTOs
                var categoryDtos = categories.Select(MapToDto).ToList();

                return new GetCategoriesResponseDto
                {
                    Categories = categoryDtos,
                    Pagination = new PaginationInfo
                    {
                        CurrentPage = request.PageNumber,
                        TotalPages = totalPages,
                        PageSize = request.PageSize,
                        TotalCount = totalCount,
                        SearchTerm = request.SearchTerm,
                        HasNextPage = hasNextPage,
                        HasPreviousPage = hasPreviousPage,
                        StartItem = startItem,
                        EndItem = endItem
                    }
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving categories: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets a specific category by ID
        /// </summary>
        public async Task<CategoryDto?> GetCategoryByIdAsync(Guid categoryId)
        {
            try
            {
                var category = await _categoryRepository.GetFirstOrDefault(
                    c => c.Id == categoryId,
                    "Products"
                );

                return category != null ? MapToDto(category) : null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving category: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Creates a new category
        /// </summary>
        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
        {
            try
            {
                // Check if name already exists
                if (await CategoryNameExistsAsync(createCategoryDto.Name))
                {
                    throw new InvalidOperationException("اسم الصنف موجود مسبقاً");
                }

                var category = new Category
                {
                    Id = Guid.NewGuid(),
                    Name = createCategoryDto.Name.Trim()
                };

                await _categoryRepository.AddAsync(category);
                await _categoryRepository.SaveChangesAsync();

                return MapToDto(category);
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates an existing category
        /// </summary>
        public async Task<CategoryDto?> UpdateCategoryAsync(Guid categoryId, UpdateCategoryDto updateCategoryDto)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(categoryId);
                if (category == null)
                {
                    return null;
                }

                // Check if name already exists (excluding current category)
                if (await CategoryNameExistsAsync(updateCategoryDto.Name, categoryId))
                {
                    throw new InvalidOperationException("اسم الصنف موجود مسبقاً");
                }

                category.Name = updateCategoryDto.Name.Trim();

                _categoryRepository.Update(category);
                await _categoryRepository.SaveChangesAsync();

                return await GetCategoryByIdAsync(categoryId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating category: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deletes a category
        /// </summary>
        public async Task<bool> DeleteCategoryAsync(Guid categoryId)
        {
            try
            {
                var category = await _categoryRepository.GetFirstOrDefault(
                    c => c.Id == categoryId,
                    "Products"
                );

                if (category == null)
                {
                    return false;
                }

                // Check if category has products
                if (category.Products != null && category.Products.Any())
                {
                    throw new InvalidOperationException("لا يمكن حذف الصنف لأنه يحتوي على منتجات");
                }

                _categoryRepository.Delete(category);
                await _categoryRepository.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting category: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets category statistics for dashboard
        /// </summary>
        public async Task<CategoryStatisticsDto> GetCategoryStatisticsAsync()
        {
            try
            {
                var categories = await _categoryRepository.GetAllAsync("Products");
                var totalCategories = categories.Count();

                var categoriesWithProducts = categories.Count(c => c.Products != null && c.Products.Any());
                var emptyCategories = totalCategories - categoriesWithProducts;

                var today = DateTime.Today;
                var startOfMonth = new DateTime(today.Year, today.Month, 1);
                var createdThisMonth = categories.Count(c => c.Id != Guid.Empty); // Assuming creation date tracking

                var topCategory = categories
                    .Where(c => c.Products != null)
                    .OrderByDescending(c => c.Products!.Count())
                    .FirstOrDefault();

                var totalProducts = categories.Sum(c => c.Products?.Count() ?? 0);
                var averageProductsPerCategory = totalCategories > 0 ? (double)totalProducts / totalCategories : 0;

                return new CategoryStatisticsDto
                {
                    TotalCategories = totalCategories,
                    CategoriesWithProducts = categoriesWithProducts,
                    EmptyCategories = emptyCategories,
                    CreatedThisMonth = createdThisMonth,
                    TopCategoryName = topCategory?.Name,
                    TopCategoryProductCount = topCategory?.Products?.Count() ?? 0,
                    AverageProductsPerCategory = Math.Round(averageProductsPerCategory, 2)
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving category statistics: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Checks if category name exists
        /// </summary>
        public async Task<bool> CategoryNameExistsAsync(string name, Guid? excludeId = null)
        {
            try
            {
                var normalizedName = name.Trim().ToLower();

                if (excludeId.HasValue)
                {
                    return await _categoryRepository.ExistsAsync(c =>
                        c.Name.ToLower() == normalizedName && c.Id != excludeId.Value);
                }

                return await _categoryRepository.ExistsAsync(c =>
                    c.Name.ToLower() == normalizedName);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking category name existence: {ex.Message}", ex);
            }
        }

        #region Private Methods

        /// <summary>
        /// Builds filter expression based on request parameters
        /// </summary>
        private Expression<Func<Category, bool>>? BuildFilterExpression(GetCategoriesRequestDto request)
        {
            Expression<Func<Category, bool>>? filter = null;

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTermLower = request.SearchTerm.Trim().ToLower();
                filter = c => c.Name.ToLower().Contains(searchTermLower);
            }

            return filter;
        }

        /// <summary>
        /// Builds order expression based on request parameters
        /// </summary>
        private Func<IQueryable<Category>, IOrderedQueryable<Category>> BuildOrderExpression(GetCategoriesRequestDto request)
        {
            return request.SortBy?.ToLower() switch
            {
                "name" => request.SortDirection?.ToLower() == "desc"
                    ? q => q.OrderByDescending(c => c.Name)
                    : q => q.OrderBy(c => c.Name),
                "productcount" => request.SortDirection?.ToLower() == "desc"
                    ? q => q.OrderByDescending(c => c.Products.Count())
                    : q => q.OrderBy(c => c.Products.Count()),
                _ => request.SortDirection?.ToLower() == "desc"
                    ? q => q.OrderByDescending(c => c.Name)
                    : q => q.OrderBy(c => c.Name)
            };
        }

        /// <summary>
        /// Maps Category entity to CategoryDto
        /// </summary>
        private CategoryDto MapToDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                ProductCount = category.Products?.Count() ?? 0,
            };
        }

        #endregion
    }
}

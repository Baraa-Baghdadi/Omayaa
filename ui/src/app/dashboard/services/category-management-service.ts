import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
export interface CategoryDto {
  id: string;
  name: string;
  productCount: number;
  createdAt: Date;
  updatedAt?: Date;
}

export interface CreateCategoryDto {
  name: string;
}

export interface UpdateCategoryDto {
  name: string;
}

export interface GetCategoriesRequestDto {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  sortBy?: string;
  sortDirection?: string;
  includeProductCount?: boolean;
}

export interface PaginationInfo {
  currentPage: number;
  totalPages: number;
  pageSize: number;
  totalCount: number;
  searchTerm?: string;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
  startItem: number;
  endItem: number;
}

export interface GetCategoriesResponseDto {
  categories: CategoryDto[];
  pagination: PaginationInfo;
}

export interface CategoryStatisticsDto {
  totalCategories: number;
  categoriesWithProducts: number;
  emptyCategories: number;
  createdThisMonth: number;
  topCategoryName?: string;
  topCategoryProductCount: number;
  averageProductsPerCategory: number;
}

@Injectable({
  providedIn: 'root'
})
export class CategoryManagementService {
  private apiUrl = environment.API_URL + "api/Category/";

  constructor(private http: HttpClient) { }

  /**
   * Get all categories with advanced filtering and pagination
   * @param request Filter and pagination parameters
   * @returns Observable of GetCategoriesResponseDto
   */
  getAllCategories(request: GetCategoriesRequestDto = {}): Observable<GetCategoriesResponseDto> {
    let params = new HttpParams();
    
    if (request.pageNumber) {
      params = params.set('pageNumber', request.pageNumber.toString());
    }
    if (request.pageSize) {
      params = params.set('pageSize', request.pageSize.toString());
    }
    if (request.searchTerm && request.searchTerm.trim()) {
      params = params.set('searchTerm', request.searchTerm.trim());
    }
    if (request.sortBy) {
      params = params.set('sortBy', request.sortBy);
    }
    if (request.sortDirection) {
      params = params.set('sortDirection', request.sortDirection);
    }
    if (request.includeProductCount !== undefined) {
      params = params.set('includeProductCount', request.includeProductCount.toString());
    }

    return this.http.get<GetCategoriesResponseDto>(
      this.apiUrl,
      { params }
    );
  }

  /**
   * Get a specific category by ID
   * @param categoryId Category unique identifier
   * @returns Observable of CategoryDto
   */
  getCategoryById(categoryId: string): Observable<CategoryDto> {
    return this.http.get<CategoryDto>(
      this.apiUrl + categoryId
    );
  }

  /**
   * Create a new category
   * @param createCategoryDto Category creation data
   * @returns Observable of CategoryDto
   */
  createCategory(createCategoryDto: CreateCategoryDto): Observable<CategoryDto> {
    return this.http.post<CategoryDto>(
      this.apiUrl,
      createCategoryDto
    );
  }

  /**
   * Update an existing category
   * @param categoryId Category ID to update
   * @param updateCategoryDto Category update data
   * @returns Observable of CategoryDto
   */
  updateCategory(categoryId: string, updateCategoryDto: UpdateCategoryDto): Observable<CategoryDto> {
    return this.http.put<CategoryDto>(
      this.apiUrl + categoryId,
      updateCategoryDto
    );
  }

  /**
   * Delete a category
   * @param categoryId Category ID to delete
   * @returns Observable of boolean
   */
  deleteCategory(categoryId: string): Observable<boolean> {
    return this.http.delete<boolean>(
      this.apiUrl + categoryId
    );
  }

  /**
   * Get category statistics for dashboard
   * @returns Observable of CategoryStatisticsDto
   */
  getCategoryStatistics(): Observable<CategoryStatisticsDto> {
    return this.http.get<CategoryStatisticsDto>(
      this.apiUrl + 'statistics'
    );
  }

  /**
   * Check if category name exists
   * @param name Category name to check
   * @param excludeId Category ID to exclude from check (optional)
   * @returns Observable of boolean
   */
  checkCategoryNameExists(name: string, excludeId?: string): Observable<boolean> {
    let params = new HttpParams().set('name', name);
    
    if (excludeId) {
      params = params.set('excludeId', excludeId);
    }

    return this.http.get<boolean>(
      this.apiUrl + 'check-name',
      { params }
    );
  }
}

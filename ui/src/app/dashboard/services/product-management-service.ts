import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ProductDto {
  id: string;
  name: string;
  price: number;
  newPrice?: number;
  imageUrl: string;
  isActive: boolean;
  categoryId: string;
  categoryName: string;
  createdAt: Date;
}

export interface CreateProductDto {
  name: string;
  price: number;
  newPrice?: number;
  image?: File;
  isActive: boolean;
  categoryId: string;
}

export interface UpdateProductDto {
  name: string;
  price: number;
  newPrice?: number;
  image?: File;
  isActive: boolean;
  categoryId: string;
}

export interface GetProductsRequestDto {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  categoryId?: string;
  isActive?: boolean;
  minPrice?: number;
  maxPrice?: number;
  sortBy?: string;
  sortDirection?: string;
  includeCategoryInfo?: boolean;
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

export interface GetProductsResponseDto {
  products: ProductDto[];
  pagination: PaginationInfo;
}

export interface ProductStatisticsDto {
  totalProducts: number;
  activeProducts: number;
  inactiveProducts: number;
  productsWithDiscount: number;
  createdThisMonth: number;
  averagePrice: number;
  highestPrice: number;
  lowestPrice: number;
  topCategoryName?: string;
  topCategoryProductCount: number;
}

export interface CategoryDto {
  id: string;
  name: string;
  productCount: number;
  createdAt: Date;
  updatedAt?: Date;
}

@Injectable({
  providedIn: 'root'
})
export class ProductManagementService {
  private apiUrl = environment.API_URL + "api/Product/";
  private categoryApiUrl = environment.API_URL + "api/Category/";

  constructor(private http: HttpClient) { }

  /**
   * Get all products with advanced filtering and pagination
   * @param request Filter and pagination parameters
   * @returns Observable of GetProductsResponseDto
   */
  getAllProducts(request: GetProductsRequestDto = {}): Observable<GetProductsResponseDto> {
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
    if (request.categoryId) {
      params = params.set('categoryId', request.categoryId);
    }
    if (request.isActive !== undefined) {
      params = params.set('isActive', request.isActive.toString());
    }
    if (request.minPrice !== undefined) {
      params = params.set('minPrice', request.minPrice.toString());
    }
    if (request.maxPrice !== undefined) {
      params = params.set('maxPrice', request.maxPrice.toString());
    }
    if (request.sortBy) {
      params = params.set('sortBy', request.sortBy);
    }
    if (request.sortDirection) {
      params = params.set('sortDirection', request.sortDirection);
    }
    if (request.includeCategoryInfo !== undefined) {
      params = params.set('includeCategoryInfo', request.includeCategoryInfo.toString());
    }

    return this.http.get<GetProductsResponseDto>(this.apiUrl, { params });
  }

  /**
   * Get product by ID
   * @param productId Product unique identifier
   * @returns Observable of ProductDto
   */
  getProductById(productId: string): Observable<ProductDto> {
    return this.http.get<ProductDto>(`${this.apiUrl}${productId}`);
  }

  /**
   * Create a new product
   * @param createProductDto Product creation data
   * @returns Observable of ProductDto
   */
  createProduct(createProductDto: CreateProductDto): Observable<ProductDto> {
    const formData = new FormData();
    formData.append('name', createProductDto.name);
    formData.append('price', createProductDto.price.toString());
    if (createProductDto.newPrice !== undefined && createProductDto.newPrice !== null) {
      formData.append('newPrice', createProductDto.newPrice.toString());
    }
    if (createProductDto.image) {
      formData.append('image', createProductDto.image);
    }
    formData.append('isActive', createProductDto.isActive.toString());
    formData.append('categoryId', createProductDto.categoryId);

    return this.http.post<ProductDto>(this.apiUrl, formData);
  }

  /**
   * Update an existing product
   * @param productId Product ID to update
   * @param updateProductDto Product update data
   * @returns Observable of ProductDto
   */
  updateProduct(productId: string, updateProductDto: UpdateProductDto): Observable<ProductDto> {
    const formData = new FormData();
    formData.append('name', updateProductDto.name);
    formData.append('price', updateProductDto.price.toString());
    if (updateProductDto.newPrice !== undefined && updateProductDto.newPrice !== null) {
      formData.append('newPrice', updateProductDto.newPrice.toString());
    }
    if (updateProductDto.image) {
      formData.append('image', updateProductDto.image);
    }
    formData.append('isActive', updateProductDto.isActive.toString());
    formData.append('categoryId', updateProductDto.categoryId);

    return this.http.put<ProductDto>(`${this.apiUrl}${productId}`, formData);
  }

  /**
   * Delete a product
   * @param productId Product ID to delete
   * @returns Observable of boolean
   */
  deleteProduct(productId: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}${productId}`);
  }

  /**
   * Get product statistics
   * @returns Observable of ProductStatisticsDto
   */
  getProductStatistics(): Observable<ProductStatisticsDto> {
    return this.http.get<ProductStatisticsDto>(`${this.apiUrl}statistics`);
  }

  /**
   * Get all categories for dropdowns
   * @returns Observable of CategoryDto array
   */
  getAllCategories(): Observable<{ categories: CategoryDto[] }> {
    return this.http.get<{ categories: CategoryDto[] }>(this.categoryApiUrl);
  }
}

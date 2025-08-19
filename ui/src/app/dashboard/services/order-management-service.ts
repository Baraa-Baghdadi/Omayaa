import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

/**
 * Order Data Transfer Objects - Matching the backend DTOs
 */
export interface OrderDto {
  id: string;
  orderNumber: string;
  providerId: string;
  providerName: string;
  totalAmount: number;
  discountAmount: number;
  status : OrderStatus;
  finalAmount: number;
  notes: string;
  orderDate: Date;
  deliveryDate?: Date;
  createdAt: Date;
  updatedAt?: Date;
  orderItems: OrderItemDto[];
  totalItems: number;
}

export interface OrderItemDto {
  id: string;
  orderId: string;
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  notes: string;
  createdAt: Date;
}

export interface CreateOrderDto {
  providerId: string;
  discountAmount: number;
  notes: string;
  deliveryDate?: Date;
  orderItems: CreateOrderItemDto[];
}

export interface CreateOrderItemDto {
  productId: string;
  quantity: number;
  notes: string;
}

export interface UpdateOrderDto {
  discountAmount: number;
  notes: string;
  deliveryDate?: Date;
  orderItems: UpdateOrderItemDto[];
}

export interface UpdateOrderItemDto {
  id?: string; // null for new items
  productId: string;
  quantity: number;
  notes: string;
}

export interface GetOrdersRequestDto {
  pageNumber: number;
  pageSize: number;
  searchTerm: string;
  providerId?: string;
  startDate?: Date;
  endDate?: Date;
  sortBy: string;
  sortDirection: string;
  includeOrderItems: boolean;
}

export interface GetOrdersResponseDto {
  orders: OrderDto[];
  totalCount: number;
  totalPages: number;
  currentPage: number;
  pageSize: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface OrderStatisticsDto {
  totalOrders: number;
  totalRevenue: number;
  monthlyRevenue: number;
  averageOrderValue: number;
  monthlyRevenueData: MonthlyRevenueDto[];
}

export interface MonthlyRevenueDto {
  month: string;
  revenue: number;
  orderCount: number;
}

/**
 * Order Status Enum - matching backend
 */
export enum OrderStatus {
  New = 0,
  Completed = 1,
  Canceled = 2
}

export interface UpdateOrderStatusDto {
  orderId: string;
  newStatus: OrderStatus;
}

/**
 * Order Management Service - Professional service for handling all order operations
 * Provides comprehensive CRUD operations and statistics for order management
 */
@Injectable({
  providedIn: 'root'
})
export class OrderManagementService {
   private readonly apiUrl = `${environment.API_URL}api/Order`;

  constructor(private http: HttpClient) {}

  /**
   * Retrieves all orders with advanced filtering and pagination
   * @param request Filter and pagination parameters
   * @returns Observable of paginated orders response
   */
  getAllOrders(request: GetOrdersRequestDto): Observable<GetOrdersResponseDto> {
    let params = new HttpParams()
      .set('PageNumber', request.pageNumber.toString())
      .set('PageSize', request.pageSize.toString())
      .set('SearchTerm', request.searchTerm || '')
      .set('SortBy', request.sortBy || 'OrderDate')
      .set('SortDirection', request.sortDirection || 'desc')
      .set('IncludeOrderItems', request.includeOrderItems.toString());

    // Add optional parameters if they exist
    if (request.providerId) {
      params = params.set('ProviderId', request.providerId);
    }
    if (request.startDate) {
      params = params.set('StartDate', request.startDate.toISOString());
    }
    if (request.endDate) {
      params = params.set('EndDate', request.endDate.toISOString());
    }

    return this.http.get<GetOrdersResponseDto>(this.apiUrl, { params })
      .pipe(
        catchError(this.handleError)
      );
  }

  /**
   * Retrieves a specific order by ID with all details
   * @param orderId Order unique identifier
   * @returns Observable of order details
   */
  getOrderById(orderId: string): Observable<OrderDto> {
    return this.http.get<OrderDto>(`${this.apiUrl}/${orderId}`)
      .pipe(
        catchError(this.handleError)
      );
  }

  /**
   * Retrieves a specific order by order number
   * @param orderNumber Order number
   * @returns Observable of order details
   */
  getOrderByNumber(orderNumber: string): Observable<OrderDto> {
    return this.http.get<OrderDto>(`${this.apiUrl}/by-number/${orderNumber}`)
      .pipe(
        catchError(this.handleError)
      );
  }

  /**
   * Creates a new order with order items
   * @param createOrderDto Order creation data
   * @returns Observable of created order
   */
  createOrder(createOrderDto: CreateOrderDto): Observable<OrderDto> {
    return this.http.post<OrderDto>(this.apiUrl, createOrderDto)
      .pipe(
        catchError(this.handleError)
      );
  }

  /**
   * Updates an existing order and its items
   * @param orderId Order ID to update
   * @param updateOrderDto Order update data
   * @returns Observable of updated order
   */
  updateOrder(orderId: string, updateOrderDto: UpdateOrderDto): Observable<OrderDto> {
    return this.http.put<OrderDto>(`${this.apiUrl}/${orderId}`, updateOrderDto)
      .pipe(
        catchError(this.handleError)
      );
  }

  
  /**
   * Updates order status
   * @param updateStatusDto Order status update data
   * @returns Observable of boolean success status
   */
  updateOrderStatus(updateStatusDto: UpdateOrderStatusDto): Observable<boolean> {
    return this.http.put<boolean>(`${this.apiUrl}/UpdateOrderStatus`, updateStatusDto)
      .pipe(
        catchError(this.handleError)
      );
  }

  /**
   * Deletes an order and all its items (Admin only)
   * @param orderId Order ID to delete
   * @returns Observable of boolean success status
   */
  deleteOrder(orderId: string): Observable<boolean> {
    return this.http.delete<boolean>(`${this.apiUrl}/${orderId}`)
      .pipe(
        catchError(this.handleError)
      );
  }

  /**
   * Gets comprehensive order statistics for admin dashboard
   * @returns Observable of order statistics
   */
  getOrderStatistics(): Observable<OrderStatisticsDto> {
    return this.http.get<OrderStatisticsDto>(`${this.apiUrl}/statistics`)
      .pipe(
        catchError(this.handleError)
      );
  }

  /**
   * Gets orders for a specific provider
   * @param providerId Provider ID
   * @param request Filtering and pagination parameters
   * @returns Observable of provider's orders
   */
  getOrdersByProvider(providerId: string, request: GetOrdersRequestDto): Observable<GetOrdersResponseDto> {
    let params = new HttpParams()
      .set('PageNumber', request.pageNumber.toString())
      .set('PageSize', request.pageSize.toString())
      .set('SearchTerm', request.searchTerm || '')
      .set('SortBy', request.sortBy || 'OrderDate')
      .set('SortDirection', request.sortDirection || 'desc')
      .set('IncludeOrderItems', request.includeOrderItems.toString());

    return this.http.get<GetOrdersResponseDto>(`${this.apiUrl}/provider/${providerId}`, { params })
      .pipe(
        catchError(this.handleError)
      );
  }

  /**
   * Checks if an order number exists
   * @param orderNumber Order number to check
   * @param excludeId Order ID to exclude from check (for updates)
   * @returns Observable of boolean existence status
   */
  orderNumberExists(orderNumber: string, excludeId?: string): Observable<boolean> {
    let params = new HttpParams()
      .set('orderNumber', orderNumber);

    if (excludeId) {
      params = params.set('excludeId', excludeId);
    }

    return this.http.get<boolean>(`${this.apiUrl}/exists`, { params })
      .pipe(
        catchError(this.handleError)
      );
  }

  /**
   * Enhanced error handling for HTTP requests
   * @param error HTTP error response
   * @returns Observable error with user-friendly message
   */
  private handleError(error: any): Observable<never> {
    let errorMessage = 'حدث خطأ غير متوقع';

    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = `خطأ في الشبكة: ${error.error.message}`;
    } else {
      // Server-side error
      switch (error.status) {
        case 400:
          errorMessage = error.error?.message || 'بيانات غير صحيحة';
          break;
        case 401:
          errorMessage = 'غير مصرح بالوصول';
          break;
        case 403:
          errorMessage = 'ليس لديك صلاحية للقيام بهذا الإجراء';
          break;
        case 404:
          errorMessage = 'الطلب غير موجود';
          break;
        case 409:
          errorMessage = 'تعارض في البيانات';
          break;
        case 500:
          errorMessage = 'خطأ في الخادم';
          break;
        default:
          errorMessage = `خطأ: ${error.status} - ${error.error?.message || error.message}`;
      }
    }

    console.error('Order Management Service Error:', error);
    return throwError(() => new Error(errorMessage));
  }
}

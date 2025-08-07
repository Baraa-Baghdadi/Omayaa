import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../enviroments/environment.development';

// DTOs matching backend interfaces
export interface DashboardCardsDto {
  totalOrders: number;
  totalRevenue: number;
  numberOfProviders: number;
  numberOfProducts: number;
  newOrdersToday: number;
}

export interface OrderAnalyticsDto {
  totalOrdersDaily: number;
  totalOrdersMonthly: number;
  totalOrdersYearly: number;
  totalRevenueDaily: number;
  totalRevenueMonthly: number;
  averageOrderValue: number;
  averagePreparationTimeHours: number;
}

export interface OrdersByStatusDto {
  status: OrderStatus;
  statusName: string;
  count: number;
}

export interface BestSellingProductDto {
  productId: string;
  productName: string;
  categoryName: string;
  totalSold: number;
  totalRevenue: number;
}

export interface ProductCategoryDistributionDto {
  categoryId: string;
  categoryName: string;
  productCount: number;
}

export interface OrderTrendDto {
  date: Date;
  orderCount: number;
  revenue: number;
}

export interface LatestOrderDto {
  id: string;
  orderNumber: string;
  providerName: string;
  finalAmount: number;
  status: OrderStatus;
  orderDate: Date;
  itemCount: number;
}

export interface ChartDatasetDto {
  label: string;
  data: any[];
  backgroundColor: string[];
  borderColor: string[];
  borderWidth: number;
}

export interface ChartDataDto {
  labels: string[];
  datasets: ChartDatasetDto[];
}

export interface PieChartDataDto {
  labels: string[];
  data: number[];
  backgroundColors: string[];
}

export enum OrderStatus {
  Completed = 1,
  Cancelled = 2,
}

@Injectable({
  providedIn: 'root'
})
export class DashboardAdminService {
   private readonly apiUrl = `${environment.API_URL}api/AdminDashboard`;

  constructor(private http: HttpClient) {}

  /**
   * Get dashboard summary cards
   */
  getDashboardCards(): Observable<DashboardCardsDto> {
    return this.http.get<DashboardCardsDto>(`${this.apiUrl}/cards`)
      .pipe(catchError(this.handleError));
  }

  /**
   * Get order analytics
   */
  getOrderAnalytics(): Observable<OrderAnalyticsDto> {
    return this.http.get<OrderAnalyticsDto>(`${this.apiUrl}/analytics/orders`)
      .pipe(catchError(this.handleError));
  }

  /**
   * Get orders by status
   */
  getOrdersByStatus(): Observable<OrdersByStatusDto[]> {
    return this.http.get<OrdersByStatusDto[]>(`${this.apiUrl}/analytics/orders-by-status`)
      .pipe(catchError(this.handleError));
  }

  /**
   * Get best selling products
   */
  getBestSellingProducts(take: number = 10): Observable<BestSellingProductDto[]> {
    const params = new HttpParams().set('take', take.toString());
    return this.http.get<BestSellingProductDto[]>(`${this.apiUrl}/analytics/best-selling-products`, { params })
      .pipe(catchError(this.handleError));
  }

  /**
   * Get least selling products
   */
  getLeastSellingProducts(take: number = 10): Observable<BestSellingProductDto[]> {
    const params = new HttpParams().set('take', take.toString());
    return this.http.get<BestSellingProductDto[]>(`${this.apiUrl}/analytics/least-selling-products`, { params })
      .pipe(catchError(this.handleError));
  }

  /**
   * Get product category distribution
   */
  getProductCategoryDistribution(): Observable<ProductCategoryDistributionDto[]> {
    return this.http.get<ProductCategoryDistributionDto[]>(`${this.apiUrl}/analytics/product-category-distribution`)
      .pipe(catchError(this.handleError));
  }

  /**
   * Get order trends
   */
  getOrderTrends(period: string = 'daily', days: number = 30): Observable<OrderTrendDto[]> {
    const params = new HttpParams()
      .set('period', period)
      .set('days', days.toString());
    return this.http.get<OrderTrendDto[]>(`${this.apiUrl}/analytics/order-trends`, { params })
      .pipe(catchError(this.handleError));
  }

  /**
   * Get latest orders
   */
  getLatestOrders(take: number = 10): Observable<LatestOrderDto[]> {
    const params = new HttpParams().set('take', take.toString());
    return this.http.get<LatestOrderDto[]>(`${this.apiUrl}/tables/latest-orders`, { params })
      .pipe(catchError(this.handleError));
  }

  // Chart.js endpoints
  /**
   * Get best selling products chart data
   */
  getBestSellingProductsChart(): Observable<ChartDataDto> {
    return this.http.get<ChartDataDto>(`${this.apiUrl}/charts/best-selling-products`)
      .pipe(catchError(this.handleError));
  }

  /**
   * Get order count by day chart data
   */
  getOrderCountByDayChart(days: number = 30): Observable<ChartDataDto> {
    const params = new HttpParams().set('days', days.toString());
    return this.http.get<ChartDataDto>(`${this.apiUrl}/charts/orders-by-day`, { params })
      .pipe(catchError(this.handleError));
  }

  /**
   * Get revenue trend chart data
   */
  getRevenueTrendChart(days: number = 30): Observable<ChartDataDto> {
    const params = new HttpParams().set('days', days.toString());
    return this.http.get<ChartDataDto>(`${this.apiUrl}/charts/revenue-trends`, { params })
      .pipe(catchError(this.handleError));
  }

  /**
   * Get order status breakdown chart data
   */
  getOrderStatusBreakdownChart(): Observable<PieChartDataDto> {
    return this.http.get<PieChartDataDto>(`${this.apiUrl}/charts/order-status-breakdown`)
      .pipe(catchError(this.handleError));
  }

  /**
   * Get product category distribution chart data
   */
  getProductCategoryDistributionChart(): Observable<ChartDataDto> {
    return this.http.get<ChartDataDto>(`${this.apiUrl}/charts/product-category-distribution`)
      .pipe(catchError(this.handleError));
  }

  /**
   * Handle HTTP errors
   */
  private handleError(error: any): Observable<never> {
    console.error('Dashboard service error:', error);
    return throwError(() => new Error(error?.error?.message || 'حدث خطأ في تحميل البيانات'));
  }

  /**
   * Get order status text in Arabic
   */
  getOrderStatusText(status: OrderStatus): string {
    const statusMap: { [key in OrderStatus]: string } = {
      [OrderStatus.Completed]: 'مكتمل',
      [OrderStatus.Cancelled]: 'ملغي',
    };
    return statusMap[status] || 'غير محدد';
  }

  /**
   * Get order status CSS class
   */
  getOrderStatusClass(status: OrderStatus): string {
    const statusClasses: { [key in OrderStatus]: string } = {
      [OrderStatus.Completed]: 'badge bg-success',
      [OrderStatus.Cancelled]: 'badge bg-danger',
    };
    return statusClasses[status] || 'badge bg-secondary';
  }
}

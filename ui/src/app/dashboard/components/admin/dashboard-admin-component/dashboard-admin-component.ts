import { Component, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, forkJoin } from 'rxjs';
import { takeUntil, finalize } from 'rxjs/operators';
import { Chart, registerables } from 'chart.js';
import { BestSellingProductDto, DashboardAdminService, DashboardCardsDto, LatestOrderDto, OrderAnalyticsDto, OrdersByStatusDto, OrderStatus, ProductCategoryDistributionDto } from '../../../services/dashboard-admin-service';

Chart.register(...registerables);

@Component({
  selector: 'app-dashboard-admin-component',
  imports: [CommonModule, FormsModule],
  templateUrl: './dashboard-admin-component.html',
  styleUrl: './dashboard-admin-component.scss'
})
export class DashboardAdminComponent implements OnInit, OnDestroy{
 @ViewChild('revenueChart', { static: true }) revenueChartRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('statusChart', { static: true }) statusChartRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('bestSellingChart', { static: true }) bestSellingChartRef!: ElementRef<HTMLCanvasElement>;
  @ViewChild('categoryChart', { static: true }) categoryChartRef!: ElementRef<HTMLCanvasElement>;

  private destroy$ = new Subject<void>();
  
  // Data properties
  dashboardCards?: DashboardCardsDto;
  orderAnalytics?: OrderAnalyticsDto;
  bestSellingProducts: BestSellingProductDto[] = [];
  latestOrders: LatestOrderDto[] = [];
  ordersByStatus: OrdersByStatusDto[] = [];
  categoryDistribution: ProductCategoryDistributionDto[] = [];
  
  // Chart instances
  private revenueChart?: Chart;
  private statusChart?: Chart;
  private bestSellingChart?: Chart;
  private categoryChart?: Chart;
  
  // UI states
  loading = false;
  revenuePeriod = 30;

  constructor(private dashboardService: DashboardAdminService) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.destroyCharts();
  }

  /**
   * Load all dashboard data
   */
  loadDashboardData(): void {
    this.loading = true;
    const requests = forkJoin({
      cards: this.dashboardService.getDashboardCards(),
      analytics: this.dashboardService.getOrderAnalytics(),
      bestSelling: this.dashboardService.getBestSellingProducts(10),
      latestOrders: this.dashboardService.getLatestOrders(10),
      ordersByStatus: this.dashboardService.getOrdersByStatus(),
    });

    requests.pipe(
      takeUntil(this.destroy$),
      finalize(() => this.loading = false)
    ).subscribe({
      next: (data) => {
        this.dashboardCards = data.cards;
        this.orderAnalytics = data.analytics;
        this.bestSellingProducts = data.bestSelling;
        this.latestOrders = data.latestOrders;
        this.ordersByStatus = data.ordersByStatus;
        
        // Update charts with new data
        this.updateCharts();
      },
      error: (error) => {
        console.error('Error loading dashboard data:', error);
      }
    });
  }

  /**
   * Refresh dashboard data
   */
  refreshDashboard(): void {
    this.loadDashboardData();
  }


  /**
   * Update all charts with new data
   */
  private updateCharts(): void {
    this.loadRevenueChart();
    this.updateStatusChart();
    this.updateBestSellingChart();
    this.updateCategoryChart();
  }

  /**
   * Initialize revenue trend chart
   */
  private initializeRevenueChart(): void {
    if (!this.revenueChartRef?.nativeElement) return;

    const ctx = this.revenueChartRef.nativeElement.getContext('2d');
    if (!ctx) return;

    this.revenueChart = new Chart(ctx, {
      type: 'line',
      data: {
        labels: [],
        datasets: []
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: 'top'
          }
        },
        scales: {
          y: {
            beginAtZero: true
          }
        }
      }
    });
  }

  /**
   * Load revenue chart data
   */
  loadRevenueChart(): void {
    this.dashboardService.getRevenueTrendChart(this.revenuePeriod)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          console.log(data,"data");
          
          if (this.revenueChart) {
            this.revenueChart.data = {
              labels: data.labels,
              datasets: data.datasets.map(dataset => ({
                ...dataset,
                backgroundColor: '#FFCC0220',
                borderColor: '#FFCC02',
                borderWidth: 3,
                tension: 0.4
              }))
            };
            this.revenueChart.update();
          }
        },
        error: (error) => console.error('Error loading revenue chart:', error)
      });
  }

  /**
   * Initialize status chart
   */
  private initializeStatusChart(): void {
    if (!this.statusChartRef?.nativeElement) return;

    const ctx = this.statusChartRef.nativeElement.getContext('2d');
    if (!ctx) return;

    this.statusChart = new Chart(ctx, {
      type: 'doughnut',
      data: {
        labels: [],
        datasets: []
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: 'bottom'
          }
        }
      }
    });
  }

  /**
   * Update status chart
   */
  private updateStatusChart(): void {
    if (!this.statusChart) return;

    this.dashboardService.getOrderStatusBreakdownChart()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          if (this.statusChart) {
            this.statusChart.data = {
              labels: data.labels,
              datasets: [{
                data: data.data,
                backgroundColor: [
                  '#FFC107', '#17A2B8', '#007BFF', 
                  '#28A745', '#FFCC02', '#DC3545', '#6C757D'
                ]
              }]
            };
            this.statusChart.update();
          }
        },
        error: (error) => console.error('Error loading status chart:', error)
      });
  }

  /**
   * Initialize best selling chart
   */
  private initializeBestSellingChart(): void {
    if (!this.bestSellingChartRef?.nativeElement) return;

    const ctx = this.bestSellingChartRef.nativeElement.getContext('2d');
    if (!ctx) return;

    this.bestSellingChart = new Chart(ctx, {
      type: 'bar',
      data: {
        labels: [],
        datasets: []
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: false
          }
        },
        scales: {
          y: {
            beginAtZero: true
          }
        }
      }
    });
  }

  /**
   * Update best selling chart
   */
  private updateBestSellingChart(): void {
    if (!this.bestSellingChart) return;

    this.dashboardService.getBestSellingProductsChart()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          if (this.bestSellingChart) {
            this.bestSellingChart.data = {
              labels: data.labels,
              datasets: data.datasets.map(dataset => ({
                ...dataset,
                backgroundColor: '#2c3e50',
                borderColor: '#FFCC02',
                borderWidth: 1
              }))
            };
            this.bestSellingChart.update();
          }
        },
        error: (error) => console.error('Error loading best selling chart:', error)
      });
  }

  /**
   * Initialize category chart
   */
  private initializeCategoryChart(): void {
    if (!this.categoryChartRef?.nativeElement) return;

    const ctx = this.categoryChartRef.nativeElement.getContext('2d');
    if (!ctx) return;

    this.categoryChart = new Chart(ctx, {
      type: 'pie',
      data: {
        labels: [],
        datasets: []
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: 'right'
          }
        }
      }
    });
  }

  /**
   * Update category chart
   */
  private updateCategoryChart(): void {
    if (!this.categoryChart) return;

    this.dashboardService.getProductCategoryDistributionChart()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          if (this.categoryChart) {
            this.categoryChart.data = {
              labels: data.labels,
              datasets: data.datasets.map(dataset => ({
                ...dataset,
                backgroundColor: [
                  '#FFCC02', '#2c3e50', '#28A745', 
                  '#DC3545', '#17A2B8', '#FFC107', '#6C757D'
                ]
              }))
            };
            this.categoryChart.update();
          }
        },
        error: (error) => console.error('Error loading category chart:', error)
      });
  }

  /**
   * Destroy all chart instances
   */
  private destroyCharts(): void {
    if (this.revenueChart) {
      this.revenueChart.destroy();
    }
    if (this.statusChart) {
      this.statusChart.destroy();
    }
    if (this.bestSellingChart) {
      this.bestSellingChart.destroy();
    }
    if (this.categoryChart) {
      this.categoryChart.destroy();
    }
  }

  /**
   * Get order status text
   */
  getOrderStatusText(status: OrderStatus): string {
    return this.dashboardService.getOrderStatusText(status);
  }

  /**
   * Get order status CSS class
   */
  getOrderStatusClass(status: OrderStatus): string {
    return this.dashboardService.getOrderStatusClass(status);
  }
}

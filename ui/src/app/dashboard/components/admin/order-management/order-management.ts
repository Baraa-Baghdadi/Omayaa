
import { Component, OnInit, ViewChild, OnDestroy } from '@angular/core';
import { CommonModule, NgClass } from '@angular/common';
import { FormBuilder, FormsModule, FormGroup, Validators, NgModel, FormControl } from '@angular/forms';
import { NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';
import { 
  OrderManagementService, 
  OrderDto, 
  GetOrdersRequestDto, 
  OrderStatisticsDto,
  CreateOrderDto,
  UpdateOrderDto,
} from '../../../services/order-management-service';
import { ModalConfig, ModalResult, SharedModalComponent } from '../../../../shared/components/shared-modal-component/shared-modal-component';
import { SharedModalService } from '../../../../shared/services/shared-modal-service';
import { ErrorPopup } from '../../../../shared/services/error-popup';
import { debounceTime, distinctUntilChanged, Subject, takeUntil } from 'rxjs';
import { ProviderDropdownDto, ProviderManagementService } from '../../../services/provider-management-service';

// Declare Bootstrap for TypeScript
declare var bootstrap: any;

@Component({
  selector: 'app-order-management',
  imports: [CommonModule, NgClass, FormsModule, NgbDropdownModule, SharedModalComponent],
  templateUrl: './order-management.html',
  styleUrl: './order-management.scss'
})
export class OrderManagement {
 @ViewChild(SharedModalComponent) modalComponent!: SharedModalComponent;

  // Subject for managing subscriptions and preventing memory leaks
  private destroy$ = new Subject<void>();
  private searchSubject = new Subject<string>();

  // Flag to prevent duplicate API calls
  private isLoadingOrders = false;
  private isLoadingStatistics = false;
  private isProcessingModal = false;

  // Expose Math for template calculations
  Math = Math;
  
  // Core data properties
  orders: OrderDto[] = [];
  statistics: OrderStatisticsDto | null = null;
  loading = false;
  error: string | null = null;

  // Filter and pagination properties
  searchTerm = '';
  selectedProviderId = '';
  startDate = '';
  endDate = '';
  currentPage = 1;
  pageSize = 10;
  totalPages = 0;
  totalCount = 0;
  sortBy = 'OrderDate';
  sortDirection = 'desc';

  // UI state properties
  showFilters = false;
  selectedOrder: OrderDto | null = null;

  // Action loading states for individual operations
  actionLoading: { [key: string]: boolean } = {};

  // Modal configuration and state
  modalConfig: ModalConfig | null = null;
  modalForm: FormGroup | null = null;
  modalLoading = false;
  modalSaving = false;
  modalError: string | null = null;

  providers: ProviderDropdownDto[] = [];

  // Sort options for orders
  sortOptions = [
    { value: 'OrderDate', label: 'تاريخ الطلب' },
    { value: 'OrderNumber', label: 'رقم الطلب' },
    { value: 'ProviderName', label: 'اسم المزود' },
    { value: 'TotalAmount', label: 'إجمالي المبلغ' },
    { value: 'FinalAmount', label: 'المبلغ النهائي' }
  ];

  // Pagination computed properties
  get pageNumbers(): number[] {
    const pages: number[] = [];
    const start = Math.max(1, this.currentPage - 2);
    const end = Math.min(this.totalPages, this.currentPage + 2);
    
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  }

  constructor(
    private orderService: OrderManagementService,
    private modalService: SharedModalService,
    private providerService: ProviderManagementService,
    private errorPopup: ErrorPopup,
    private fb: FormBuilder
  ) {
    // Initialize empty form to prevent null reference errors
    this.modalForm = this.fb.group({});
  }

  ngOnInit(): void {
    this.loadOrders();
    this.loadStatistics();
    this.loadProviders();
    this.initializeModal();
    this.setupSearchSubscription();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Sets up reactive search functionality to avoid excessive API calls
   */
  private setupSearchSubscription(): void {
    this.searchSubject
      .pipe(
        debounceTime(500),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        this.currentPage = 1;
        this.loadOrders();
      });
  }

  /**
   * Initializes Bootstrap modal for order operations
   */
  private initializeModal(): void {
    setTimeout(() => {
      const modalElement = document.getElementById('orderModal');
      if (modalElement) {
        // Modal initialization can be handled by SharedModalComponent
      }
    }, 100);
  }

  /**
   * Loads orders with current filter and pagination settings
   */
  loadOrders(): void {
    if (this.isLoadingOrders) return;
    
    this.isLoadingOrders = true;
    this.loading = true;
    this.error = null;

    const request: GetOrdersRequestDto = {
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      searchTerm: this.searchTerm,
      sortBy: this.sortBy,
      sortDirection: this.sortDirection,
      includeOrderItems: true,
      providerId: this.selectedProviderId || undefined,
      startDate: this.startDate ? new Date(this.startDate) : undefined,
      endDate: this.endDate ? new Date(this.endDate) : undefined
    };

    this.orderService.getAllOrders(request)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.orders = response.orders;
          this.totalCount = response.totalCount;
          this.totalPages = response.totalPages;
          this.currentPage = response.currentPage;
          this.loading = false;
          this.isLoadingOrders = false;
        },
        error: (error) => {
          this.error = error.message;
          this.loading = false;
          this.isLoadingOrders = false;
          // this.errorPopup.show('خطأ في تحميل الطلبات', error.message);
        }
      });
  }

    /**
   * Loads providers for dropdown selection
   */
  loadProviders(): void {
    this.providerService.getProvidersForDropdown(false) // Only active providers
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (providers) => {
          this.providers = providers;
        },
        error: (error) => {
          console.error('Error loading providers for dropdown:', error);
          // Don't show error popup for this as it's not critical
        }
      });
  }

  /**
   * Loads order statistics for dashboard display
   */
  loadStatistics(): void {
    if (this.isLoadingStatistics) return;
    
    this.isLoadingStatistics = true;

    this.orderService.getOrderStatistics()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (stats) => {
          this.statistics = stats;
          this.isLoadingStatistics = false;
        },
        error: (error) => {
          console.error('Error loading order statistics:', error);
          this.isLoadingStatistics = false;
        }
      });
  }

  /**
   * Handles search input changes with debouncing
   */
  onSearchChange(): void {
    this.searchSubject.next(this.searchTerm);
  }

  /**
   * Handles filter changes and reloads data
   */
  onFilterChange(): void {
    this.currentPage = 1;
    this.loadOrders();
  }

  /**
   * Handles sort option changes
   */
  onSortChange(): void {
    this.currentPage = 1;
    this.loadOrders();
  }

  /**
   * Clears all filters and resets to default state
   */
  clearFilters(): void {
    this.searchTerm = '';
    this.selectedProviderId = '';
    this.startDate = '';
    this.endDate = '';
    this.sortBy = 'OrderDate';
    this.sortDirection = 'desc';
    this.currentPage = 1;
    this.loadOrders();
  }

  /**
   * Navigation methods for pagination
   */
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
      this.currentPage = page;
      this.loadOrders();
    }
  }

  goToFirstPage(): void {
    this.goToPage(1);
  }

  goToPreviousPage(): void {
    this.goToPage(this.currentPage - 1);
  }

  goToNextPage(): void {
    this.goToPage(this.currentPage + 1);
  }

  goToLastPage(): void {
    this.goToPage(this.totalPages);
  }

  /**
   * Opens modal for editing an existing order
   */
  openEditOrderModal(order: OrderDto): void {
    this.selectedOrder = order;
    
    const modalConfig: ModalConfig = {
      title: 'تعديل الطلب',
      subtitle: `تعديل بيانات الطلب رقم: ${order.orderNumber}`,
      fields: [
        {
          key: 'discountAmount',
          label: 'مبلغ الخصم',
          type: 'number',
          placeholder: '0.00',
          required: false
        },
        {
          key: 'deliveryDate',
          label: 'تاريخ التسليم',
          type: 'date',
          required: false
        },
        {
          key: 'notes',
          label: 'ملاحظات',
          type: 'textarea',
          placeholder: 'ملاحظات إضافية حول الطلب...',
          rows: 3,
          required: false
        }
      ],
      saveButtonText: 'حفظ التغييرات',
      cancelButtonText: 'إلغاء',
      size: 'lg'
    };

    const formData = {
      discountAmount: order.discountAmount,
      deliveryDate: order.deliveryDate ? new Date(order.deliveryDate).toISOString().split('T')[0] : '',
      notes: order.notes || ''
    };

    const formControls: any = {};
    modalConfig.fields.forEach(field => {
      const validators = field.required ? [Validators.required] : [];
      formControls[field.key] = [formData[field.key as keyof typeof formData] || '', validators];
    });

    const form = this.fb.group(formControls);
    this.modalService.openModal(modalConfig, form);

    // MANUALLY trigger modal show after a short delay
    setTimeout(() => {
      if (this.modalComponent) {
        this.modalComponent.show();
      }
      this.isProcessingModal = false;
    }, 200);
  }

  /**
   * Handles modal cancel operations
   */
  onModalCancel(): void {
    this.modalService.closeModal();
    this.selectedOrder = null;
    this.isProcessingModal = false;
  }

  /**
   * Updates an existing order
   */
  private updateOrder(orderId: string, formData: any): void {
    const updateOrderDto: UpdateOrderDto = {
      discountAmount: formData.discountAmount || 0,
      notes: formData.notes || '',
      deliveryDate: formData.deliveryDate ? new Date(formData.deliveryDate) : undefined,
      orderItems: [] // This should be populated with current order items
    };

    this.orderService.updateOrder(orderId, updateOrderDto)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (updatedOrder) => {
          this.loadOrders();
          this.loadStatistics();
          this.modalService.closeModal();
          this.selectedOrder = null;
          this.isProcessingModal = false;
          // this.errorPopup.show('نجح', 'تم تحديث الطلب بنجاح', 'success');
        },
        error: (error) => {
          this.modalService.setError(error.message);
          this.modalService.setSaving(false);
          this.isProcessingModal = false;
        }
      });
  }

  /**
   * Deletes an order with confirmation
   */
  deleteOrder(order: OrderDto): void {
    if (!confirm(`هل أنت متأكد من حذف الطلب رقم ${order.orderNumber}؟`)) {
      return;
    }

    const actionKey = `delete_${order.id}`;
    this.actionLoading[actionKey] = true;

    this.orderService.deleteOrder(order.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.loadOrders();
          this.loadStatistics();
          this.actionLoading[actionKey] = false;
          // this.errorPopup.show('نجح', 'تم حذف الطلب بنجاح', 'success');
        },
        error: (error) => {
          this.actionLoading[actionKey] = false;
          // this.errorPopup.show('خطأ في حذف الطلب', error.message);
        }
      });
  }

  /**
   * Formats currency for display
   */
formatCurrency(amount: number): string {
  return new Intl.NumberFormat('ar-SY', {
    style: 'currency',
    currency: 'SYP',
    minimumFractionDigits: 0,
    maximumFractionDigits: 2
  }).format(amount);
}

  /**
   * Formats date for display
   */
  formatDate(date: Date | string): string {
    const dateObj = typeof date === 'string' ? new Date(date) : date;
    return new Intl.DateTimeFormat('ar-SA', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit'
    }).format(dateObj);
  }

  /**
   * Gets display status for an order based on various conditions
   */
  getOrderStatus(order: OrderDto): { text: string; class: string } {
    const now = new Date();
    const orderDate = new Date(order.orderDate);
    const deliveryDate = order.deliveryDate ? new Date(order.deliveryDate) : null;

    if (deliveryDate && deliveryDate < now) {
      return { text: 'مُسلّم', class: 'text-success' };
    } else if (deliveryDate && deliveryDate > now) {
      return { text: 'قيد التجهيز', class: 'text-warning' };
    } else {
      return { text: 'جديد', class: 'text-info' };
    }
  }
}

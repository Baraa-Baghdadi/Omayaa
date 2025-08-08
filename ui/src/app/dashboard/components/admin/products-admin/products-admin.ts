import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule, NgClass } from '@angular/common';
import { FormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';
import { ProductManagementService, ProductDto, GetProductsRequestDto, ProductStatisticsDto, CreateProductDto, UpdateProductDto, CategoryDto } from '../../../services/product-management-service';
import { SharedModalComponent, ModalConfig } from '../../../../shared/components/shared-modal-component/shared-modal-component';
import { SharedModalService } from '../../../../shared/services/shared-modal-service';
import { ErrorPopup } from '../../../../shared/services/error-popup';
import { debounceTime, distinctUntilChanged, Subject, takeUntil } from 'rxjs';
import { environment } from '../../../../../environments/environment';

@Component({
  selector: 'app-products-admin',
   standalone: true,
  imports: [CommonModule, NgClass, FormsModule, NgbDropdownModule, SharedModalComponent],
  templateUrl: './products-admin.html',
  styleUrl: './products-admin.scss'
})
export class ProductsAdmin {
  @ViewChild(SharedModalComponent) modalComponent!: SharedModalComponent;

  // Subject for managing subscriptions
  private destroy$ = new Subject<void>();
  private searchSubject = new Subject<string>();

  // Flag to prevent duplicate API calls
  private isLoadingProducts = false;
  private isLoadingStatistics = false;
  private isProcessingModal = false;

  // Expose Math for template
  Math = Math;
  
  // Data properties
  products: ProductDto[] = [];
  categories: CategoryDto[] = [];
  statistics: ProductStatisticsDto | null = null;
  loading = false;
  error: string | null = null;

  // Filter and pagination properties
  searchTerm = '';
  selectedCategory = '';
  selectedStatus = '';
  minPrice = '';
  maxPrice = '';
  currentPage = 1;
  pageSize = 10;
  totalPages = 0;
  totalCount = 0;
  sortBy = 'CreatedAt';
  sortDirection = 'desc';

  // UI properties
  showFilters = false;
  selectedProduct: ProductDto | null = null;

  // Action loading states
  actionLoading: { [key: string]: boolean } = {};

  // Modal properties
  modalConfig: ModalConfig | null = null;
  modalForm: FormGroup | null = null;
  modalLoading = false;
  modalSaving = false;
  modalError: string | null = null;

  // Status options
  statusOptions = [
    { value: '', label: 'جميع المنتجات' },
    { value: 'true', label: 'منتجات نشطة' },
    { value: 'false', label: 'منتجات غير نشطة' }
  ];

  constructor(
    private productService: ProductManagementService,
    private modalService: SharedModalService,
    private errorPopup: ErrorPopup,
    private fb: FormBuilder
  ) {
    this.modalForm = this.fb.group({});
  }

  ngOnInit(): void {
    this.loadProducts();
    this.loadStatistics();
    this.loadCategories();
    this.setupSearchDebounce();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Setup search debounce to avoid excessive API calls
   */
  private setupSearchDebounce(): void {
    this.searchSubject
      .pipe(
        debounceTime(500),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        this.currentPage = 1;
        this.loadProducts();
      });
  }

  /**
   * Load products with current filters
   */
  async loadProducts(): Promise<void> {
    if (this.isLoadingProducts) return;
    
    this.isLoadingProducts = true;
    this.loading = true;
    this.error = null;

    try {
      const request: GetProductsRequestDto = {
        pageNumber: this.currentPage,
        pageSize: this.pageSize,
        searchTerm: this.searchTerm.trim() || undefined,
        categoryId: this.selectedCategory || undefined,
        isActive: this.selectedStatus !== '' ? this.selectedStatus === 'true' : undefined,
        minPrice: this.minPrice ? parseInt(this.minPrice) : undefined,
        maxPrice: this.maxPrice ? parseInt(this.maxPrice) : undefined,
        sortBy: this.sortBy,
        sortDirection: this.sortDirection,
        includeCategoryInfo: true
      };

      const response = await this.productService.getAllProducts(request).toPromise();
      
      if (response) {
        this.products = response.products;
        this.totalPages = response.pagination.totalPages;
        this.totalCount = response.pagination.totalCount;
      }
    } catch (error: any) {
      this.error = error.error?.message || 'حدث خطأ أثناء تحميل المنتجات';
      // this.errorPopup.showError(this.error);
    } finally {
      this.loading = false;
      this.isLoadingProducts = false;
    }
  }

  /**
   * Load product statistics
   */
  async loadStatistics(): Promise<void> {
    if (this.isLoadingStatistics) return;
    
    this.isLoadingStatistics = true;

    try {
      this.statistics = await this.productService.getProductStatistics().toPromise() || null;
    } catch (error: any) {
      console.error('Error loading statistics:', error);
    } finally {
      this.isLoadingStatistics = false;
    }
  }

  /**
   * Load categories for dropdowns
   */
  async loadCategories(): Promise<void> {
    try {
      const response = await this.productService.getAllCategories().toPromise();
      if (response) {
        this.categories = response.categories;
      }
    } catch (error: any) {
      console.error('Error loading categories:', error);
    }
  }

  /**
   * Handle search input change
   */
  onSearchChange(): void {
    this.searchSubject.next(this.searchTerm);
  }

  /**
   * Handle filter change
   */
  onFilterChange(): void {
    this.currentPage = 1;
    this.loadProducts();
  }

  /**
   * Clear all filters
   */
  clearFilters(): void {
    this.searchTerm = '';
    this.selectedCategory = '';
    this.selectedStatus = '';
    this.minPrice = '';
    this.maxPrice = '';
    this.currentPage = 1;
    this.loadProducts();
  }

  /**
   * Handle sorting
   */
  onSort(column: string): void {
    if (this.sortBy === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortBy = column;
      this.sortDirection = 'asc';
    }
    this.currentPage = 1;
    this.loadProducts();
  }

  /**
   * Get sort icon for column
   */
  getSortIcon(column: string): string {
    if (this.sortBy !== column) {
      return 'fas fa-sort';
    }
    return this.sortDirection === 'asc' ? 'fas fa-sort-up' : 'fas fa-sort-down';
  }

  /**
   * Handle pagination
   */
  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
      this.currentPage = page;
      this.loadProducts();
    }
  }

  /**
   * Get page numbers for pagination
   */
  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxVisiblePages = 5;
    let startPage = Math.max(1, this.currentPage - Math.floor(maxVisiblePages / 2));
    let endPage = Math.min(this.totalPages, startPage + maxVisiblePages - 1);

    if (endPage - startPage + 1 < maxVisiblePages) {
      startPage = Math.max(1, endPage - maxVisiblePages + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }

    return pages;
  }

  /**
   * Open create product modal
   */
  openCreateProductModal(): void {
     if (this.isProcessingModal) return;
    this.isProcessingModal = true;

    // First set up the config and form
    this.modalConfig = {
      title: 'إضافة منتج جديد',
      subtitle: 'أدخل تفاصيل المنتج الجديد',
      fields: [
        {
          key: 'name',
          label: 'اسم المنتج',
          type: 'text',
          placeholder: 'أدخل اسم المنتج',
          required: true,
          validation: { maxLength: 150 }
        },
        {
          key: 'categoryId',
          label: 'الصنف',
          type: 'select',
          required: true,
          options: this.categories.map(cat => ({ value: cat.id, label: cat.name }))
        },
        {
          key: 'price',
          label: 'السعر',
          type: 'number',
          placeholder: 'أدخل السعر',
          required: true,
          validation: { min: 0,max:1000000 }
        },
        {
          key: 'newPrice',
          label: 'السعر المخفض (اختياري)',
          type: 'number',
          placeholder: 'أدخل السعر المخفض',
          validation: { min: 0,max:1000000 }
        },
        {
          key: 'image',
          label: 'صورة المنتج (اختياري)',
          type: 'file',
          accept: 'image/*',
          validation: {
            maxFileSize: 5 * 1024 * 1024, // 5MB
            allowedFileTypes: ['jpg', 'jpeg', 'png', 'gif', 'webp']
          }
        },
        {
          key: 'isActive',
          label: 'المنتج نشط',
          type: 'checkbox'
        }
      ],
      saveButtonText: 'إضافة المنتج',
      cancelButtonText: 'إلغاء'
    };

    this.modalForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(150)]],
      categoryId: ['', Validators.required],
      price: ['', [Validators.required, Validators.min(0)]],
      newPrice: [''],
      image: [null],
      isActive: [true]
    });

    this.modalError = null;
    this.modalSaving = false;
    this.selectedProduct = null;
    
    // MANUALLY trigger modal show after a short delay
    setTimeout(() => {
      if (this.modalComponent) {
        this.modalComponent.show();
      }
      this.isProcessingModal = false;
    }, 200);
  }

  /**
   * Open edit product modal
   */
 // Update the openEditProductModal() method similarly:
openEditProductModal(product: ProductDto): void {
  if (this.isProcessingModal) return;
  this.isProcessingModal = true;

  this.selectedProduct = product;

  this.modalConfig = {
    title: 'تعديل المنتج',
    subtitle: `تعديل تفاصيل المنتج: ${product.name}`,
    fields: [
      {
        key: 'name',
        label: 'اسم المنتج',
        type: 'text',
        placeholder: 'أدخل اسم المنتج',
        required: true,
        validation: { maxLength: 150 }
      },
      {
        key: 'categoryId',
        label: 'الصنف',
        type: 'select',
        required: true,
        options: this.categories.map(cat => ({ value: cat.id, label: cat.name }))
      },
      {
        key: 'price',
        label: 'السعر',
        type: 'number',
        placeholder: 'أدخل السعر',
        required: true,
        validation: { min: 0,max:1000000 }
      },
      {
        key: 'newPrice',
        label: 'السعر المخفض (اختياري)',
        type: 'number',
        placeholder: 'أدخل السعر المخفض',
        validation: { min: 0,max:1000000 }
      },
      {
        key: 'image',
        label: 'صورة المنتج الجديدة (اختياري)',
        type: 'file',
        accept: 'image/*',
        validation: {
          maxFileSize: 5 * 1024 * 1024, // 5MB
          allowedFileTypes: ['jpg', 'jpeg', 'png', 'gif', 'webp']
        }
      },
      {
        key: 'isActive',
        label: 'المنتج نشط',
        type: 'checkbox'
      }
    ],
    saveButtonText: 'حفظ التغييرات',
    cancelButtonText: 'إلغاء'
  };

  this.modalForm = this.fb.group({
    name: [product.name, [Validators.required, Validators.maxLength(150)]],
    categoryId: [product.categoryId, Validators.required],
    price: [product.price, [Validators.required, Validators.min(0)]],
    newPrice: [product.newPrice || ''],
    image: [null],
    isActive: [product.isActive]
  });

  this.modalError = null;
  this.modalSaving = false;
  this.isProcessingModal = false;

      // MANUALLY trigger modal show after a short delay
    setTimeout(() => {
      if (this.modalComponent) {
        this.modalComponent.show();
      }
      this.isProcessingModal = false;
    }, 200);
}

  /**
   * Open delete product modal
   */
  openDeleteProductModal(product: ProductDto): void {
    if (this.isProcessingModal) return;
    this.isProcessingModal = true;

    this.selectedProduct = product;

    this.modalConfig = {
      title: 'تأكيد حذف المنتج',
      subtitle: `هل أنت متأكد من حذف المنتج: ${product.name}؟`,
      fields: [],
      saveButtonText: 'حذف المنتج',
      cancelButtonText: 'إلغاء',
      saveButtonClass: 'btn-danger'
    };

    this.modalForm = this.fb.group({});
    this.modalError = null;
    this.modalSaving = false;
    this.isProcessingModal = false;

    // MANUALLY trigger modal show after a short delay
    setTimeout(() => {
      if (this.modalComponent) {
        this.modalComponent.show();
      }
      this.isProcessingModal = false;
    }, 200);
  }

  /**
   * Handle modal save action
   */
  async onModalSave(): Promise<void> {
    if (!this.modalForm || !this.modalConfig) return;

    // Handle delete action
    if (this.modalConfig.title === 'تأكيد حذف المنتج' && this.selectedProduct) {
      await this.deleteProduct(this.selectedProduct.id);
      return;
    }

    if (!this.modalForm.valid) {
      this.modalError = 'يرجى التأكد من صحة جميع البيانات المدخلة';
      return;
    }

    this.modalSaving = true;
    this.modalError = null;

    try {
      const formData = this.modalForm.value;

      if (this.selectedProduct) {
        // Update existing product
        const updateDto: UpdateProductDto = {
          name: formData.name,
          price: formData.price,
          newPrice: formData.newPrice || undefined,
          image: formData.image,
          isActive: formData.isActive,
          categoryId: formData.categoryId
        };

        await this.productService.updateProduct(this.selectedProduct.id, updateDto).toPromise();
      } else {
        // Create new product
        const createDto: CreateProductDto = {
          name: formData.name,
          price: formData.price,
          newPrice: formData.newPrice || undefined,
          image: formData.image,
          isActive: formData.isActive,
          categoryId: formData.categoryId
        };

        await this.productService.createProduct(createDto).toPromise();
      }

      // Reload data and close modal
      await this.loadProducts();
      await this.loadStatistics();
      this.closeModal();

    } catch (error: any) {
      this.modalError = error.error?.message || 'حدث خطأ أثناء العملية';
    } finally {
      this.modalSaving = false;
    }
  }

  /**
   * Delete product
   */
  async deleteProduct(productId: string): Promise<void> {
    this.modalSaving = true;
    this.modalError = null;

    try {
      await this.productService.deleteProduct(productId).toPromise();
      await this.loadProducts();
      await this.loadStatistics();
      this.closeModal();
    } catch (error: any) {
      this.modalError = error.error?.message || 'حدث خطأ أثناء حذف المنتج';
    } finally {
      this.modalSaving = false;
    }
  }

  /**
   * Handle file input change
   */
  onFileChange(event: any): void {
    const file = event.target.files[0];
    if (file && this.modalForm) {
      this.modalForm.patchValue({
        image: file
      });
    }
  }

  /**
   * Close modal
   */
  closeModal(): void {
    this.modalConfig = null;
    this.modalForm = null;
    this.selectedProduct = null;
    this.modalError = null;
    this.modalSaving = false;
  }

  /**
   * Format currency
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
   * Get product status badge class
   */
  getStatusBadgeClass(isActive: boolean): string {
    return isActive ? 'badge bg-success' : 'badge bg-secondary';
  }

  /**
   * Get product status text
   */
  getStatusText(isActive: boolean): string {
    return isActive ? 'نشط' : 'غير نشط';
  }

  /**
   * Get category name for dropdown
   */
  getCategoryName(categoryId: string): string {
    const category = this.categories.find(c => c.id === categoryId);
    return category ? category.name : 'غير محدد';
  }

  /**
   * Handle image error
   */
  onImageError(event: any): void {
    event.target.src = 'assets/images/no-image.png';
  }

  /**
   * Get product image URL with fallback
   */
  getProductImageUrl(product: ProductDto): string {
    if (!product.imageUrl || product.imageUrl.trim() === '') {
      return 'assets/images/no-image.png';
    }
    
    // If imageUrl starts with http, return as is
    if (product.imageUrl.startsWith('http')) {
      return product.imageUrl;
    }
    
    // If imageUrl is relative path from backend, construct full URL
    if (product.imageUrl.startsWith('/')) {
      return `${environment.API_URL.replace('/api/', '')}${product.imageUrl}`;
    }
    
    // Default case - assume it's a relative path
    return `${environment.API_URL.replace('/api/', '')}/${product.imageUrl}`;
  }
}

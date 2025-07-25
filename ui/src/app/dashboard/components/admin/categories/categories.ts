
import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule, NgClass } from '@angular/common';
import { FormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';
import { CategoryManagementService, CategoryDto, GetCategoriesRequestDto, CategoryStatisticsDto, CreateCategoryDto, UpdateCategoryDto } from '../../../services/category-management-service';
import { SharedModalComponent, ModalConfig, ModalResult } from '../../../../shared/components/shared-modal-component/shared-modal-component';
import { SharedModalService } from '../../../../shared/services/shared-modal-service';
import { ErrorPopup } from '../../../../shared/services/error-popup';
import { debounceTime, distinctUntilChanged, Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-categories',
   standalone: true,
  imports: [CommonModule, NgClass, FormsModule, NgbDropdownModule, SharedModalComponent],
  templateUrl: './categories.html',
  styleUrl: './categories.scss'
})
export class Categories {
    @ViewChild(SharedModalComponent) modalComponent!: SharedModalComponent;

  // Subject for managing subscriptions
  private destroy$ = new Subject<void>();
  private searchSubject = new Subject<string>();

  // Flag to prevent duplicate API calls
  private isLoadingCategories = false;
  private isLoadingStatistics = false;
  private isProcessingModal = false; // إضافة flag جديد لمنع معالجة المودال المزدوجة

  // Expose Math for template
  Math = Math;
  
  // Data properties
  categories: CategoryDto[] = [];
  statistics: CategoryStatisticsDto | null = null;
  loading = false;
  error: string | null = null;

  // Filter and pagination properties
  searchTerm = '';
  currentPage = 1;
  pageSize = 10;
  totalPages = 0;
  totalCount = 0;
  sortBy = 'Name';
  sortDirection = 'asc';

  // UI properties
  showFilters = false;
  selectedCategory: CategoryDto | null = null;

  // Action loading states
  actionLoading: { [key: string]: boolean } = {};

  // Modal properties
  modalConfig: ModalConfig | null = null;
  modalForm: FormGroup | null = null;
  modalLoading = false;
  modalSaving = false;
  modalError: string | null = null;

  constructor(
    private categoryService: CategoryManagementService,
    private modalService: SharedModalService,
    private errorPopup: ErrorPopup,
    private fb: FormBuilder
  ) {
    this.modalForm = this.fb.group({});
  }

  ngOnInit(): void {
    this.initializeSearchDebounce();
    this.loadInitialData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initializeSearchDebounce(): void {
    this.searchSubject
      .pipe(
        debounceTime(500),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe((searchTerm:any) => {
        this.searchTerm = searchTerm;
        this.currentPage = 1;
        this.loadCategories();
      });
  }

  private async loadInitialData(): Promise<void> {
    try {
      await Promise.all([
        this.loadCategories(),
        this.loadStatistics()
      ]);
    } catch (error) {
      console.error('Error loading initial data:', error);
    }
  }

  async loadCategories(): Promise<void> {
    if (this.isLoadingCategories) {
      return;
    }

    this.isLoadingCategories = true;
    this.loading = true;
    this.error = null;

    try {
      const request: GetCategoriesRequestDto = {
        pageNumber: this.currentPage,
        pageSize: this.pageSize,
        searchTerm: this.searchTerm.trim() || undefined,
        sortBy: this.sortBy,
        sortDirection: this.sortDirection,
        includeProductCount: true
      };

      const response = await this.categoryService.getAllCategories(request).toPromise();
      
      if (response) {
        this.categories = response.categories;
        this.totalPages = response.pagination.totalPages;
        this.totalCount = response.pagination.totalCount;
        this.currentPage = response.pagination.currentPage;
      }
    } catch (error: any) {
      console.error('Error loading categories:', error);
      this.error = 'حدث خطأ في تحميل البيانات';
    } finally {
      this.loading = false;
      this.isLoadingCategories = false;
    }
  }

  async loadStatistics(): Promise<void> {
    if (this.isLoadingStatistics) {
      return;
    }

    this.isLoadingStatistics = true;

    try {
      this.statistics = await this.categoryService.getCategoryStatistics().toPromise() || null;
    } catch (error) {
      console.error('Error loading statistics:', error);
    } finally {
      this.isLoadingStatistics = false;
    }
  }

  onSearchChange(searchValue: string): void {
    this.searchSubject.next(searchValue);
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
      this.currentPage = page;
      this.loadCategories();
    }
  }

  onSort(column: string): void {
    if (this.sortBy === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortBy = column;
      this.sortDirection = 'asc';
    }
    this.loadCategories();
  }

  getSortIcon(column: string): string {
    if (this.sortBy !== column) return 'fas fa-sort';
    return this.sortDirection === 'asc' ? 'fas fa-sort-up' : 'fas fa-sort-down';
  }

  // Modal Methods
  openCreateCategoryModal(): void {
    this.selectedCategory = null;
    
    this.modalConfig = {
      title: 'إضافة صنف جديد',
      subtitle: 'قم بإدخال بيانات الصنف الجديد',
      fields: [
        {
          key: 'name',
          label: 'اسم الصنف',
          type: 'text',
          required: true,
          placeholder: 'أدخل اسم الصنف',
          validation: {
            minLength: 2,
            maxLength: 100
          }
        }
      ],
      saveButtonText: 'إضافة الصنف',
      cancelButtonText: 'إلغاء',
      size: 'lg'
    };

    this.modalForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]]
    });

    this.resetModal();
    this.showModal();
  }

  openEditCategoryModal(category: CategoryDto): void {
    this.selectedCategory = category;
    
    this.modalConfig = {
      title: 'تعديل الصنف',
      subtitle: 'قم بتعديل بيانات الصنف',
      fields: [
        {
          key: 'name',
          label: 'اسم الصنف',
          type: 'text',
          required: true,
          placeholder: 'أدخل اسم الصنف',
          validation: {
            minLength: 2,
            maxLength: 100
          }
        }
      ],
      saveButtonText: 'حفظ التغييرات',
      cancelButtonText: 'إلغاء',
      size: 'lg'
    };

    this.modalForm = this.fb.group({
      name: [category.name, [Validators.required, Validators.minLength(2), Validators.maxLength(100)]]
    });

    this.resetModal();
    this.showModal();
  }

  openDeleteCategoryModal(category: CategoryDto): void {
    this.selectedCategory = category;
    
    this.modalConfig = {
      title: 'حذف الصنف',
      subtitle: `هل أنت متأكد من حذف الصنف "${category.name}"؟`,
      fields: [
        {
          key: 'confirmation',
          label: 'لتأكيد الحذف، اكتب "حذف" في المربع أدناه',
          type: 'text',
          placeholder: 'اكتب "حذف" للتأكيد',
          required: true
        }
      ],
      saveButtonText: 'حذف الصنف',
      saveButtonClass: 'btn-danger',
      cancelButtonText: 'إلغاء',
      size: 'lg'
    };

    this.modalForm = this.fb.group({
      confirmation: ['', [Validators.required]]
    });

    this.resetModal();
    this.showModal();
  }

  private resetModal(): void {
    this.modalError = null;
    this.modalSaving = false;
    this.isProcessingModal = false; // إعادة تشغيل flag المعالجة
  }

  private showModal(): void {
    setTimeout(() => {
      if (this.modalComponent) {
        this.modalComponent.show();
      }
    }, 100);
  }

  // **هنا الإصلاح الرئيسي: استخدام event handler واحد فقط**
  
  // إزالة onModalSave واستخدام onModalResult فقط
  onModalResult(result: ModalResult): void {
    if (result.action === 'save' && result.data && !this.isProcessingModal) {
      this.isProcessingModal = true; // منع المعالجة المزدوجة
      this.handleModalSave(result.data);
    } else if (result.action === 'cancel' || result.action === 'close') {
      this.onModalCancel();
    }
  }

  // **إزالة هذه الدالة تماماً لأنها تسبب الاستدعاء المزدوج**
  // onModalSave(data: any): void { ... }

  private async handleModalSave(data: any): Promise<void> {
    if (!this.modalConfig || this.modalSaving) return;

    try {
      if (this.modalConfig.title === 'إضافة صنف جديد') {
        await this.createCategory({
          name: data.name
        });
      } else if (this.modalConfig.title === 'تعديل الصنف' && this.selectedCategory) {
        await this.updateCategory(this.selectedCategory.id, {
          name: data.name
        });
      } else if (this.modalConfig.title === 'حذف الصنف' && this.selectedCategory) {
        if (data.confirmation === 'حذف') {
          await this.deleteCategory(this.selectedCategory.id);
        } else {
          this.modalError = 'يجب كتابة "حذف" للتأكيد';
          this.isProcessingModal = false;
          return;
        }
      }
    } catch (error) {
      console.error('Modal save error:', error);
      this.isProcessingModal = false;
    }
  }

  onModalCancel(): void {
    this.hideModal();
  }

  onModalClose(): void {
    this.hideModal();
  }

  private hideModal(): void {
    if (this.modalComponent) {
      this.modalComponent.hide();
    }
    this.selectedCategory = null;
    this.resetModal();
  }

  // CRUD operations
  async createCategory(data: CreateCategoryDto): Promise<void> {
    if (this.modalSaving) return;

    this.modalSaving = true;
    this.modalError = null;

    try {
      const newCategory = await this.categoryService.createCategory(data).toPromise();
      
      if (newCategory) {
        // تحديث البيانات بعد الإضافة
        await Promise.all([
          this.loadCategories(),
          this.loadStatistics()
        ]);
        
        this.hideModal();
        this.errorPopup.showSuccess();
        this.showSuccessMessage('تم إضافة الصنف بنجاح');
      }
    } catch (error: any) {
      console.error('Error creating category:', error);
      this.modalError = this.getErrorMessage(error, 'حدث خطأ في إضافة الصنف');
    } finally {
      this.modalSaving = false;
      this.isProcessingModal = false;
    }
  }

  async updateCategory(categoryId: string, data: UpdateCategoryDto): Promise<void> {
    if (this.modalSaving) return;

    this.modalSaving = true;
    this.modalError = null;

    try {
      const updatedCategory = await this.categoryService.updateCategory(categoryId, data).toPromise();
      
      if (updatedCategory) {
        // تحديث البيانات بعد التعديل
        await Promise.all([
          this.loadCategories(),
          this.loadStatistics()
        ]);
        
        this.hideModal();
        this.errorPopup.showSuccess();
        this.showSuccessMessage('تم تحديث الصنف بنجاح');
      }
    } catch (error: any) {
      console.error('Error updating category:', error);
      this.modalError = this.getErrorMessage(error, 'حدث خطأ في تحديث الصنف');
    } finally {
      this.modalSaving = false;
      this.isProcessingModal = false;
    }
  }

  async deleteCategory(categoryId: string): Promise<void> {
    if (this.modalSaving) return;

    this.modalSaving = true;
    this.modalError = null;

    try {
      await this.categoryService.deleteCategory(categoryId).toPromise();
      
      // تحديث البيانات بعد الحذف
      await Promise.all([
        this.loadCategories(),
        this.loadStatistics()
      ]);
      
      this.hideModal();
      this.errorPopup.showSuccess();
      this.showSuccessMessage('تم حذف الصنف بنجاح');
    } catch (error: any) {
      console.error('Error deleting category:', error);
      this.modalError = this.getErrorMessage(error, 'حدث خطأ في حذف الصنف');
    } finally {
      this.modalSaving = false;
      this.isProcessingModal = false;
    }
  }

  private getErrorMessage(error: any, defaultMessage: string): string {
    if (error.status === 409) {
      return 'اسم الصنف موجود مسبقاً';
    } else if (error.status === 404) {
      return 'الصنف غير موجود';
    } else if (error.status === 400) {
      return 'بيانات غير صحيحة';
    } else if (error.error && typeof error.error === 'string') {
      return error.error;
    }
    return defaultMessage;
  }

  // Utility methods
  setActionLoading(categoryId: string, loading: boolean): void {
    this.actionLoading[categoryId] = loading;
  }

  showSuccessMessage(message: string): void {
    console.log('Success:', message);
    setTimeout(() => {
      this.error = null;
    }, 3000);
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.currentPage = 1;
    this.searchSubject.next('');
  }

  formatDate(date: Date | string): string {
    if (!date) return '-';
    const d = new Date(date);
    return d.toLocaleDateString('ar-EG', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  getPaginationArray(): number[] {
    const delta = 2;
    const range = [];
    const rangeWithDots = [];

    for (let i = Math.max(2, this.currentPage - delta); 
         i <= Math.min(this.totalPages - 1, this.currentPage + delta); 
         i++) {
      range.push(i);
    }

    if (this.currentPage - delta > 2) {
      rangeWithDots.push(1, -1);
    } else {
      rangeWithDots.push(1);
    }

    rangeWithDots.push(...range);

    if (this.currentPage + delta < this.totalPages - 1) {
      rangeWithDots.push(-1, this.totalPages);
    } else {
      rangeWithDots.push(this.totalPages);
    }

    return rangeWithDots;
  }

  trackByFn(index: number, item: CategoryDto): string {
    return item.id;
  }

  toggleFilters(): void {
    this.showFilters = !this.showFilters;
  }
}

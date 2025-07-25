
import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule, NgClass } from '@angular/common';
import { FormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';
import { CategoryManagementService, CategoryDto, GetCategoriesRequestDto, CategoryStatisticsDto, CreateCategoryDto, UpdateCategoryDto } from '../../../services/category-management-service';
import { SharedModalComponent, ModalConfig, ModalResult } from '../../../../shared/components/shared-modal-component/shared-modal-component';
import { SharedModalService } from '../../../../shared/services/shared-modal-service';
import { ErrorPopup } from '../../../../shared/services/error-popup';

@Component({
  selector: 'app-categories',
   standalone: true,
  imports: [CommonModule, NgClass, FormsModule, NgbDropdownModule, SharedModalComponent],
  templateUrl: './categories.html',
  styleUrl: './categories.scss'
})
export class Categories {
  @ViewChild(SharedModalComponent) modalComponent!: SharedModalComponent;
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
  pageSize = 1;
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
    // Initialize a default form to prevent null errors
    this.modalForm = this.fb.group({});
  }

  ngOnInit(): void {
    this.loadCategories();
    this.loadStatistics();
  }

  async loadCategories(): Promise<void> {
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
      this.error = 'حدث خطأ في تحميل البيانات';
      console.error('Error loading categories:', error);
    } finally {
      this.loading = false;
    }
  }

  async loadStatistics(): Promise<void> {
    try {
      this.statistics = await this.categoryService.getCategoryStatistics().toPromise() || null;
    } catch (error) {
      console.error('Error loading statistics:', error);
    }
  }

  onSearchChange(): void {
    this.currentPage = 1;
    this.loadCategories();
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
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

    // Create form manually
    this.modalForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]]
    });

    this.modalError = null;
    this.modalSaving = false;

    setTimeout(() => {
      if (this.modalComponent) {
        this.modalComponent.show();
      }
    }, 100);
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

    // Create form with existing data
    this.modalForm = this.fb.group({
      name: [category.name, [Validators.required, Validators.minLength(2), Validators.maxLength(100)]]
    });

    this.modalError = null;
    this.modalSaving = false;

    setTimeout(() => {
      if (this.modalComponent) {
        this.modalComponent.show();
      }
    }, 100);
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

    // Create form for confirmation
    this.modalForm = this.fb.group({
      confirmation: ['', [Validators.required]]
    });

    this.modalError = null;
    this.modalSaving = false;

    setTimeout(() => {
      if (this.modalComponent) {
        this.modalComponent.show();
      }
    }, 100);
  }

  async createCategory(data: CreateCategoryDto): Promise<void> {
    this.modalSaving = true;
    this.modalError = null;

    try {
      const newCategory = await this.categoryService.createCategory(data).toPromise();
      
      if (newCategory) {
        await this.loadCategories();
        await this.loadStatistics();
        if (this.modalComponent) {
          this.modalComponent.hide();
        }
        this.errorPopup.showSuccess();
        this.showSuccessMessage('تم إضافة الصنف بنجاح');
      }
    } catch (error: any) {
      console.error('Error creating category:', error);
      let errorMessage = 'حدث خطأ في إضافة الصنف';
      
      if (error.status === 409) {
        errorMessage = 'اسم الصنف موجود مسبقاً';
      } else if (error.error && typeof error.error === 'string') {
        errorMessage = error.error;
      }
      
      this.modalError = errorMessage;
    } finally {
      this.modalSaving = false;
    }
  }

  async updateCategory(categoryId: string, data: UpdateCategoryDto): Promise<void> {
    this.modalSaving = true;
    this.modalError = null;

    try {
      const updatedCategory = await this.categoryService.updateCategory(categoryId, data).toPromise();
      
      if (updatedCategory) {
        await this.loadCategories();
        await this.loadStatistics();
        if (this.modalComponent) {
          this.modalComponent.hide();
        }
        this.errorPopup.showSuccess();
        this.showSuccessMessage('تم تحديث الصنف بنجاح');
      }
    } catch (error: any) {
      console.error('Error updating category:', error);
      let errorMessage = 'حدث خطأ في تحديث الصنف';
      
      if (error.status === 409) {
        errorMessage = 'اسم الصنف موجود مسبقاً';
      } else if (error.status === 404) {
        errorMessage = 'الصنف غير موجود';
      } else if (error.error && typeof error.error === 'string') {
        errorMessage = error.error;
      }
      
      this.modalError = errorMessage;
    } finally {
      this.modalSaving = false;
    }
  }

  async deleteCategory(categoryId: string): Promise<void> {
    this.modalSaving = true;
    this.modalError = null;

    try {
      const success = await this.categoryService.deleteCategory(categoryId).toPromise();
      
      if (success) {
        await this.loadCategories();
        await this.loadStatistics();
        if (this.modalComponent) {
          this.modalComponent.hide();
        }
        this.errorPopup.showSuccess();
        this.showSuccessMessage('تم حذف الصنف بنجاح');
      }
    } catch (error: any) {
      console.error('Error deleting category:', error);
      let errorMessage = 'حدث خطأ في حذف الصنف';
      
      if (error.status === 400 && error.error && error.error.includes('منتجات')) {
        errorMessage = 'لا يمكن حذف الصنف لأنه يحتوي على منتجات';
      } else if (error.status === 404) {
        errorMessage = 'الصنف غير موجود';
      } else if (error.error && typeof error.error === 'string') {
        errorMessage = error.error;
      }
      
      this.modalError = errorMessage;
    } finally {
      this.modalSaving = false;
    }
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
    this.loadCategories();
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

  // Modal event handlers
  onModalSave(data: any): void {
    if (!this.selectedCategory) {
      // Creating new category
      this.createCategory(data);
    } else if (this.modalConfig?.saveButtonText === 'حذف الصنف') {
      // Deleting category
      if (data.confirmation === 'حذف') {
        this.deleteCategory(this.selectedCategory.id);
      } else {
        this.modalError = 'يجب كتابة "حذف" للتأكيد';
      }
    } else {
      // Updating category
      this.updateCategory(this.selectedCategory.id, data);
    }
  }

  onModalCancel(): void {
    this.selectedCategory = null;
    this.modalError = null;
  }

  onModalClose(): void {
    this.selectedCategory = null;
    this.modalError = null;
  }

  onModalResult(result: ModalResult): void {
    if (result.action === 'save') {
      this.onModalSave(result.data);
    } else {
      this.onModalCancel();
    }
  }

  // Remove the dynamic modal properties since we're managing them directly
  // get currentModalConfig() {
  //   return this.modalService.config$;
  // }

  // get currentModalForm() {
  //   return this.modalService.form$;
  // }

  // get currentModalLoading() {
  //   return this.modalService.loading$;
  // }

  // get currentModalSaving() {
  //   return this.modalService.saving$;
  // }

  // get currentModalError() {
  //   return this.modalService.error$;
  // }
}

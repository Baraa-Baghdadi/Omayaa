import { CommonModule, NgClass } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { CategoryDto, CategoryManagementService, CategoryStatisticsDto, CreateCategoryDto, GetCategoriesRequestDto, UpdateCategoryDto } from '../../../services/category-management-service';
// Declare Bootstrap for TypeScript
declare var bootstrap: any;
@Component({
  selector: 'app-categories',
  imports: [CommonModule, NgClass, FormsModule, ReactiveFormsModule],
  templateUrl: './categories.html',
  styleUrl: './categories.scss'
})
export class Categories {
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
  
  // Modal instances
  createModal: any;
  editModal: any;
  deleteModal: any;

  // Forms
  createForm: FormGroup;
  editForm: FormGroup;

  // Form submission states
  createSubmitting = false;
  editSubmitting = false;
  deleteSubmitting = false;

  constructor(
    private categoryService: CategoryManagementService,
    private fb: FormBuilder
  ) {
    this.createForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]]
    });

    this.editForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]]
    });
  }

  ngOnInit(): void {
    this.loadCategories();
    this.loadStatistics();
    this.initializeModals();
  }

  private initializeModals(): void {
    // Initialize Bootstrap modals after view init
    setTimeout(() => {
      const createModalElement = document.getElementById('createCategoryModal');
      const editModalElement = document.getElementById('editCategoryModal');
      const deleteModalElement = document.getElementById('deleteCategoryModal');

      if (createModalElement) {
        this.createModal = new bootstrap.Modal(createModalElement, {
          backdrop: 'static',
          keyboard: false
        });
      }

      if (editModalElement) {
        this.editModal = new bootstrap.Modal(editModalElement, {
          backdrop: 'static',
          keyboard: false
        });
      }

      if (deleteModalElement) {
        this.deleteModal = new bootstrap.Modal(deleteModalElement, {
          backdrop: 'static',
          keyboard: false
        });
      }
    }, 100);
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

  clearFilters(): void {
    this.searchTerm = '';
    this.currentPage = 1;
    this.loadCategories();
  }

  // Create category methods
  openCreateModal(): void {
    this.createForm.reset();
    this.error = null;
    if (this.createModal) {
      this.createModal.show();
    }
  }

  closeCreateModal(): void {
    if (this.createModal) {
      this.createModal.hide();
    }
    this.createForm.reset();
    this.error = null;
  }

  async submitCreateForm(): Promise<void> {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }

    this.createSubmitting = true;
    this.error = null;

    try {
      const createDto: CreateCategoryDto = {
        name: this.createForm.get('name')?.value.trim()
      };

      await this.categoryService.createCategory(createDto).toPromise();
      
      this.closeCreateModal();
      await this.loadCategories();
      await this.loadStatistics();
      
    } catch (error: any) {
      if (error.status === 409) {
        this.error = 'اسم الصنف موجود مسبقاً';
      } else {
        this.error = 'حدث خطأ في إنشاء الصنف';
      }
      console.error('Error creating category:', error);
    } finally {
      this.createSubmitting = false;
    }
  }

  // Edit category methods
  openEditModal(category: CategoryDto): void {
    this.selectedCategory = category;
    this.editForm.patchValue({
      name: category.name
    });
    this.error = null;
    if (this.editModal) {
      this.editModal.show();
    }
  }

  closeEditModal(): void {
    if (this.editModal) {
      this.editModal.hide();
    }
    this.selectedCategory = null;
    this.editForm.reset();
    this.error = null;
  }

  async submitEditForm(): Promise<void> {
    if (this.editForm.invalid || !this.selectedCategory) {
      this.editForm.markAllAsTouched();
      return;
    }

    this.editSubmitting = true;
    this.error = null;

    try {
      const updateDto: UpdateCategoryDto = {
        name: this.editForm.get('name')?.value.trim()
      };

      await this.categoryService.updateCategory(this.selectedCategory.id, updateDto).toPromise();
      
      this.closeEditModal();
      await this.loadCategories();
      await this.loadStatistics();
      
    } catch (error: any) {
      if (error.status === 409) {
        this.error = 'اسم الصنف موجود مسبقاً';
      } else {
        this.error = 'حدث خطأ في تحديث الصنف';
      }
      console.error('Error updating category:', error);
    } finally {
      this.editSubmitting = false;
    }
  }

  // Delete category methods
  openDeleteModal(category: CategoryDto): void {
    this.selectedCategory = category;
    this.error = null;
    if (this.deleteModal) {
      this.deleteModal.show();
    }
  }

  closeDeleteModal(): void {
    if (this.deleteModal) {
      this.deleteModal.hide();
    }
    this.selectedCategory = null;
    this.error = null;
  }

  async confirmDelete(): Promise<void> {
    if (!this.selectedCategory) return;

    this.deleteSubmitting = true;
    this.error = null;

    try {
      await this.categoryService.deleteCategory(this.selectedCategory.id).toPromise();
      
      this.closeDeleteModal();
      await this.loadCategories();
      await this.loadStatistics();
      
    } catch (error: any) {
      if (error.status === 400) {
        this.error = 'لا يمكن حذف الصنف لأنه يحتوي على منتجات';
      } else {
        this.error = 'حدث خطأ في حذف الصنف';
      }
      console.error('Error deleting category:', error);
    } finally {
      this.deleteSubmitting = false;
    }
  }

  formatDate(date: Date | string): string {
    if (!date) return '-';
    const d = new Date(date);
    return d.toLocaleDateString('ar-EG', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      hour12: false
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

  // Form validation helpers
  isFieldInvalid(form: FormGroup, fieldName: string): boolean {
    const field = form.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getFieldError(form: FormGroup, fieldName: string): string {
    const field = form.get(fieldName);
    if (field && field.errors) {
      if (field.errors['required']) {
        return 'هذا الحقل مطلوب';
      }
      if (field.errors['minlength']) {
        return `يجب أن يكون الحد الأدنى ${field.errors['minlength'].requiredLength} أحرف`;
      }
      if (field.errors['maxlength']) {
        return `يجب أن يكون الحد الأقصى ${field.errors['maxlength'].requiredLength} حرف`;
      }
    }
    return '';
  }
}

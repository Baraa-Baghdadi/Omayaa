import { Component, OnInit } from '@angular/core';
import { CommonModule, NgClass } from '@angular/common';
import { GetProvidersRequestDto, LockAccountRequestDto, ProviderManagementDto, ProviderManagementService, ProviderStatisticsDto } from '../../../services/provider-management-service';
import { FormsModule } from '@angular/forms';
import { NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';
// Declare Bootstrap for TypeScript
declare var bootstrap: any;
@Component({
  selector: 'app-provider',
  standalone:true,
  imports: [CommonModule,NgClass,FormsModule,NgbDropdownModule],
  templateUrl: './provider.html',
  styleUrl: './provider.scss'
})

export class Provider implements OnInit {
   // Expose Math for template
  Math = Math;
  
  // Data properties
  providers: ProviderManagementDto[] = [];
  statistics: ProviderStatisticsDto | null = null;
  loading = false;
  error: string | null = null;

  // Filter and pagination properties
  searchTerm = '';
  selectedStatus = '';
  selectedVerification = '';
  currentPage = 1;
  pageSize = 10;
  totalPages = 0;
  totalCount = 0;
  sortBy = 'CreationTime';
  sortDirection = 'desc';

  // UI properties
  showFilters = false;
  selectedProvider: ProviderManagementDto | null = null;
  lockModal: any;
  lockReason = '';
  lockUntilDate = '';
  lockingProvider = false;

  // Action loading states
  actionLoading: { [key: string]: boolean } = {};

  // Status options
  statusOptions = [
    { value: '', label: 'جميع الحالات' },
    { value: 'نشط', label: 'نشط' },
    { value: 'غير نشط', label: 'غير نشط' },
    { value: 'معطل', label: 'معطل' },
    { value: 'مقفل', label: 'مقفل' },
    { value: 'بانتظار التنشيط', label: 'بانتظار التنشيط' }
  ];

  verificationOptions = [
    { value: '', label: 'جميع المزودين' },
    { value: 'true', label: 'مفعل' },
    { value: 'false', label: 'غير مفعل' }
  ];

  constructor(private providerService: ProviderManagementService) {}

  ngOnInit(): void {
    this.loadProviders();
    this.loadStatistics();
    this.initializeModal();
  }

  private initializeModal(): void {
    setTimeout(() => {
      const modalElement = document.getElementById('lockProviderModal');
      if (modalElement) {
        this.lockModal = new bootstrap.Modal(modalElement, {
          backdrop: 'static',
          keyboard: false
        });
      }
    }, 100);
  }

  async loadProviders(): Promise<void> {
    this.loading = true;
    this.error = null;

    try {
      const request: GetProvidersRequestDto = {
        pageNumber: this.currentPage,
        pageSize: this.pageSize,
        searchTerm: this.searchTerm.trim() || undefined,
        accountStatus: this.selectedStatus || undefined,
        isVerified: this.selectedVerification ? this.selectedVerification === 'true' : undefined,
        sortBy: this.sortBy,
        sortDirection: this.sortDirection
      };

      const response = await this.providerService.getAllProviders(request).toPromise();
      
      if (response) {
        this.providers = response.providers;
        this.totalPages = response.pagination.totalPages;
        this.totalCount = response.pagination.totalCount;
        this.currentPage = response.pagination.currentPage;
      }
    } catch (error: any) {
      this.error = 'حدث خطأ في تحميل البيانات';
      console.error('Error loading providers:', error);
    } finally {
      this.loading = false;
    }
  }

  async loadStatistics(): Promise<void> {
    try {
      this.statistics = await this.providerService.getProviderStatistics().toPromise() || null;
    } catch (error) {
      console.error('Error loading statistics:', error);
    }
  }

  onSearchChange(): void {
    this.currentPage = 1;
    this.loadProviders();
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadProviders();
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadProviders();
    }
  }

  onSort(column: string): void {
    if (this.sortBy === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortBy = column;
      this.sortDirection = 'desc';
    }
    this.loadProviders();
  }

  getSortIcon(column: string): string {
    if (this.sortBy !== column) return 'fas fa-sort';
    return this.sortDirection === 'asc' ? 'fas fa-sort-up' : 'fas fa-sort-down';
  }

  // Provider status methods using the service
  isProviderLocked(provider: ProviderManagementDto): boolean {
    return this.providerService.isProviderLocked(provider);
  }

  getProviderStatusText(provider: ProviderManagementDto): string {
    return this.providerService.getProviderStatus(provider);
  }

  getProviderStatusBadgeClass(provider: ProviderManagementDto): string {
    return this.providerService.getStatusBadgeClass(provider);
  }

  getProviderStatusIcon(provider: ProviderManagementDto): string {
    return this.providerService.getStatusIcon(provider);
  }

  closeLockModal(): void {
    if (this.lockModal) {
      this.lockModal.hide();
    }
    this.selectedProvider = null;
    this.lockReason = '';
    this.lockUntilDate = '';
  }

  async lockProvider(provider: ProviderManagementDto): Promise<void> {
   this.setActionLoading(provider.tenantId, false);
    
    try {
      const success = await this.providerService.unlockProvider(provider.tenantId).toPromise();

      if (success) {
        await this.loadProviders();
        await this.loadStatistics();
        this.showSuccessMessage('تم إلغاء قفل الحساب بنجاح');
      } else {
        this.error = 'فشل في إلغاء قفل الحساب';
      }
    } catch (error) {
      console.error('Error unlocking provider:', error);
      this.error = 'حدث خطأ في إلغاء قفل الحساب';
    } finally {
      this.setActionLoading(provider.tenantId, false);
    }
  }
  

  async unlockProvider(provider: ProviderManagementDto): Promise<void> {
    this.setActionLoading(provider.tenantId, true);
    
    try {
      const success = await this.providerService.unlockProvider(provider.tenantId).toPromise();

      if (success) {
        await this.loadProviders();
        await this.loadStatistics();
        this.showSuccessMessage('تم إلغاء قفل الحساب بنجاح');
      } else {
        this.error = 'فشل في إلغاء قفل الحساب';
      }
    } catch (error) {
      console.error('Error unlocking provider:', error);
      this.error = 'حدث خطأ في إلغاء قفل الحساب';
    } finally {
      this.setActionLoading(provider.tenantId, false);
    }
  }

  async toggleVerification(provider: ProviderManagementDto): Promise<void> {
    this.setActionLoading(provider.tenantId, true);
    
    try {
      const updateRequest = {
        isEmailVerified: !provider.isEmailVerified,
        isPhoneVerified: !provider.isPhoneVerified
      };

      const success = await this.providerService.updateProviderVerification(
        provider.tenantId,
        updateRequest
      ).toPromise();

      if (success) {
        await this.loadProviders();
        await this.loadStatistics();
        const action = updateRequest.isEmailVerified ? 'تفعيل' : 'إلغاء تفعيل';
        this.showSuccessMessage(`تم ${action} الحساب بنجاح`);
      } else {
        this.error = 'فشل في تحديث حالة التفعيل';
      }
    } catch (error) {
      console.error('Error updating verification:', error);
      this.error = 'حدث خطأ في تحديث حالة التفعيل';
    } finally {
      this.setActionLoading(provider.tenantId, false);
    }
  }

  viewProviderDetails(provider: ProviderManagementDto): void {
    // Implement view details functionality
    console.log('View provider details:', provider);
    // You can open a modal or navigate to a details page
  }

  // Utility methods
  setActionLoading(tenantId: string, loading: boolean): void {
    this.actionLoading[tenantId] = loading;
  }

  showSuccessMessage(message: string): void {
    // You can implement a toast notification service here
    console.log('Success:', message);
    // For now, we'll use a simple timeout to clear any existing errors
    setTimeout(() => {
      this.error = null;
    }, 3000);
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedStatus = '';
    this.selectedVerification = '';
    this.currentPage = 1;
    this.loadProviders();
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

  getMinDateTime(): string {
    const now = new Date();
    now.setMinutes(now.getMinutes() - now.getTimezoneOffset());
    return now.toISOString().slice(0, 16);
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

  trackByFn(index: number, item: ProviderManagementDto): string {
    return item.tenantId;
  }

  // Legacy methods for backwards compatibility
  getStatusBadgeClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'نشط':
      case 'active':
        return 'badge-success';
      case 'معطل':
      case 'suspended':
        return 'badge-danger';
      case 'مقفل':
      case 'locked':
        return 'badge-danger';
      case 'بانتظار التنشيط':
      case 'pending verification':
        return 'badge-warning';
      case 'غير نشط':
      case 'inactive':
        return 'badge-secondary';
      default:
        return 'badge-secondary';
    }
  }

  getStatusIcon(status: string): string {
    switch (status.toLowerCase()) {
      case 'نشط':
      case 'active':
        return 'fas fa-check-circle text-success';
      case 'معطل':
      case 'suspended':
        return 'fas fa-ban text-danger';
      case 'مقفل':
      case 'locked':
        return 'fas fa-lock text-danger';
      case 'بانتظار التنشيط':
      case 'pending verification':
        return 'fas fa-clock text-warning';
      case 'غير نشط':
      case 'inactive':
        return 'fas fa-times-circle text-secondary';
      default:
        return 'fas fa-question-circle text-secondary';
    }
  }

  // Additional methods for the new design
  toggleFilters(): void {
    this.showFilters = !this.showFilters;
  }

  // Method to get provider status in Arabic
  getProviderStatusArabic(status: string): string {
    switch (status.toLowerCase()) {
      case 'active':
        return 'نشط';
      case 'suspended':
        return 'معطل';
      case 'locked':
        return 'مقفل';
      case 'pending verification':
        return 'بانتظار التنشيط';
      case 'inactive':
        return 'غير نشط';
      default:
        return status;
    }
  }

  // Method to get short date format for the table
  getShortDate(date: Date | string): string {
    if (!date) return '-';
    const d = new Date(date);
    return d.toLocaleDateString('en-US', {
      month: 'short',
      day: '2-digit',
      year: 'numeric'
    });
  }
}

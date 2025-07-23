import { Component } from '@angular/core';
import { CommonModule, NgClass } from '@angular/common';
import { GetProvidersRequestDto, LockAccountRequestDto, ProviderManagementDto, ProviderManagementService, ProviderStatisticsDto } from '../../../services/provider-management-service';
import { FormsModule } from '@angular/forms';
// Declare Bootstrap for TypeScript
declare var bootstrap: any;
@Component({
  selector: 'app-provider',
  standalone:true,
  imports: [CommonModule,NgClass,FormsModule],
  templateUrl: './provider.html',
  styleUrl: './provider.scss'
})

export class Provider {
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
  lockModal: any; // Bootstrap modal instance
  lockReason = '';
  lockUntilDate = '';

  // Status options
  statusOptions = [
    { value: '', label: 'جميع الحالات' },
    { value: 'نشط', label: 'نشط' },
    { value: 'غير نشط', label: 'غير نشط' },
    { value: 'معطل', label: 'معطل' },
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
    // Initialize Bootstrap modal after view init
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

  getStatusBadgeClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'نشط':
      case 'active':
        return 'badge-success';
      case 'معطل':
      case 'suspended':
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

  openLockModal(provider: ProviderManagementDto): void {
    this.selectedProvider = provider;
    this.lockReason = '';
    this.lockUntilDate = '';
    
    if (this.lockModal) {
      this.lockModal.show();
    }
  }

  closeLockModal(): void {
    if (this.lockModal) {
      this.lockModal.hide();
    }
    this.selectedProvider = null;
    this.lockReason = '';
    this.lockUntilDate = '';
  }

  async toggleProviderLock(): Promise<void> {
    if (!this.selectedProvider) return;

    this.loading = true;
    
    try {
      const isCurrentlyLocked = this.isProviderLocked(this.selectedProvider);
      
      const request: LockAccountRequestDto = {
        lockUntil: isCurrentlyLocked ? null : (this.lockUntilDate ? new Date(this.lockUntilDate) : undefined),
        reason: this.lockReason || undefined
      };

      const success = await this.providerService.lockProviderAccount(
        this.selectedProvider.tenantId,
        request
      ).toPromise();

      if (success) {
        this.closeLockModal();
        await this.loadProviders();
        await this.loadStatistics();
      }
    } catch (error) {
      console.error('Error toggling provider lock:', error);
      this.error = 'حدث خطأ في تحديث حالة المزود';
    } finally {
      this.loading = false;
    }
  }

  async toggleVerification(provider: ProviderManagementDto): Promise<void> {
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
        this.loadProviders();
        this.loadStatistics();
      }
    } catch (error) {
      console.error('Error updating verification:', error);
      this.error = 'حدث خطأ في تحديث حالة التفعيل';
    }
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedStatus = '';
    this.selectedVerification = '';
    this.currentPage = 1;
    this.loadProviders();
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

  isProviderLocked(provider: ProviderManagementDto | null): boolean {
    if (!provider?.lockoutEnd) {
      return false;
    }
    
    const lockoutDate = new Date(provider.lockoutEnd);
    const now = new Date();
    
    return lockoutDate > now;
  }
}

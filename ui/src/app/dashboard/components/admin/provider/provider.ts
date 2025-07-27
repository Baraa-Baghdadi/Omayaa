import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule, NgClass } from '@angular/common';
import { GetProvidersRequestDto, LockAccountRequestDto, ProviderManagementDto, ProviderManagementService, ProviderStatisticsDto } from '../../../services/provider-management-service';
import { FormBuilder, FormGroup, FormsModule } from '@angular/forms';
import { NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';
import { ModalConfig, ModalResult, SharedModalComponent } from '../../../../shared/components/shared-modal-component/shared-modal-component';
import { debounceTime, distinctUntilChanged, Subject, takeUntil } from 'rxjs';
import { SharedModalService } from '../../../../shared/services/shared-modal-service';
import { ErrorPopup } from '../../../../shared/services/error-popup';
// Declare Bootstrap for TypeScript
declare var bootstrap: any;
@Component({
  selector: 'app-provider',
  standalone:true,
  imports: [CommonModule, NgClass, FormsModule, NgbDropdownModule, SharedModalComponent],
  templateUrl: './provider.html',
  styleUrl: './provider.scss'
})

export class Provider implements OnInit {
  @ViewChild(SharedModalComponent) modalComponent!: SharedModalComponent;

  // Subject for managing subscriptions
  private destroy$ = new Subject<void>();
  private searchSubject = new Subject<string>();

  // Flag to prevent duplicate API calls
  private isLoadingProviders = false;
  private isLoadingStatistics = false;
  private isProcessingModal = false;

  // Expose Math for template
  Math = Math;
  
  // Data properties
  providers: ProviderManagementDto[] = [];
  statistics: any | null = null;
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

  // Modal properties
  modalConfig: any = {
    title: '',
    fields: [],
    saveButtonText: 'حفظ',
    cancelButtonText: 'إلغاء',
    showCloseButton: true,
    centered: true,
    backdrop: true,
    keyboard: true
  };
  modalForm : any | null = null;
  modalLoading = false;
  modalSaving = false;
  modalError: string | null = null;

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

  constructor(
    private providerService: ProviderManagementService,
    private modalService: SharedModalService,
    private errorPopup: ErrorPopup,
    private fb: FormBuilder
  ) {
    this.modalForm = this.fb.group({});
  }

  ngOnInit(): void {
    this.loadProviders();
    this.loadStatistics();
    this.initializeModal();
    this.setupSearchSubscription();
    this.setupModalSubscriptions();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private setupSearchSubscription(): void {
    this.searchSubject
      .pipe(
        debounceTime(500),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        this.currentPage = 1;
        this.loadProviders();
      });
  }

  private setupModalSubscriptions(): void {
    // Subscribe to modal service observables
    this.modalService.config$.pipe(takeUntil(this.destroy$)).subscribe(config => {
      this.modalConfig = config;
    });

    this.modalService.form$.pipe(takeUntil(this.destroy$)).subscribe(form => {
      this.modalForm = form;
    });

    this.modalService.loading$.pipe(takeUntil(this.destroy$)).subscribe(loading => {
      this.modalLoading = loading;
    });

    this.modalService.saving$.pipe(takeUntil(this.destroy$)).subscribe(saving => {
      this.modalSaving = saving;
    });

    this.modalService.error$.pipe(takeUntil(this.destroy$)).subscribe(error => {
      this.modalError = error;
    });

    this.modalService.result$.pipe(takeUntil(this.destroy$)).subscribe(result => {
      if (result && !this.isProcessingModal) {
        this.handleModalResult(result);
      }
    });
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

  // Add new provider method
  openAddProviderModal(): void {
    const modalConfig: ModalConfig = {
      title: 'إضافة مزود جديد',
      subtitle: 'أدخل بيانات المزود الجديد',
      fields: [
        SharedModalService.createTextField('providerName', 'اسم المزود', {
          placeholder: 'أدخل اسم المزود',
          required: true,
          validation: { minLength: 2, maxLength: 100 }
        }),
        SharedModalService.createPasswordField('password', 'كلمة المرور', {
          placeholder: 'أدخل كلمة المرور',
          required: true,
          validation: { minLength: 6 }
        }),
        SharedModalService.createPasswordField('confirmPassword', 'تأكيد كلمة المرور', {
          placeholder: 'أعد إدخال كلمة المرور',
          required: true,
          validation: { minLength: 6 }
        }),
        {
          key: 'mobile',
          label: 'رقم الموبايل',
          type: 'tel',
          placeholder: '0900000000',
          required: true,
          validation: { pattern: '^[0-9]{10}$' }
        },
        {
          key: 'telephone',
          label: 'رقم الهاتف',
          type: 'tel',
          placeholder: '0110000000',
          required: false,
          validation: { pattern: '^[0-9]{10}$' }
        },
        SharedModalService.createTextareaField('address', 'العنوان', 3, {
          placeholder: 'أدخل العنوان التفصيلي',
          required: true,
          validation: { minLength: 10, maxLength: 500 }
        })
      ],
      saveButtonText: 'إنشاء الحساب',
      cancelButtonText: 'إلغاء',
      saveButtonClass: 'btn-primary',
      size: 'lg'
    };

    this.modalService.openModal(modalConfig);

        // MANUALLY trigger modal show after a short delay
    setTimeout(() => {
      if (this.modalComponent) {
        this.modalComponent.show();
      }
      this.isProcessingModal = false;
    }, 200);
  }

  handleModalResult(result: ModalResult): void {
    if (this.isProcessingModal) return;
    
    this.isProcessingModal = true;

    try {
      if (result.action === 'save' && result.data) {
        this.onModalSave(result.data);
      } else if (result.action === 'cancel' || result.action === 'close') {
        this.onModalCancel();
      }
    } finally {
      setTimeout(() => {
        this.isProcessingModal = false;
      }, 100);
    }
  }

  async onModalSave(formData: any): Promise<void> {
  if (this.isProcessingModal) return; // Prevent duplicate processing
  this.isProcessingModal = true;
    try {
      this.modalService.setSaving(true);
      this.modalService.setError(null);

      // Validate passwords match
      if (formData.password !== formData.confirmPassword) {
        this.modalService.setError('كلمة المرور وتأكيد كلمة المرور غير متطابقتين');
        return;
      }

      // Validate required fields
      if (!formData.providerName || !formData.password || !formData.mobile || !formData.address) {
        this.modalService.setError('جميع الحقول المطلوبة يجب أن تكون مُعبأة');
        return;
      }

      // Validate mobile number format
      const mobilePattern = /^[0-9]{10}$/;
      if (!mobilePattern.test(formData.mobile)) {
        this.modalService.setError('رقم الموبايل يجب أن يكون 10 أرقام فقط');
        return;
      }

      // Validate telephone if provided
      if (formData.telephone && !mobilePattern.test(formData.telephone)) {
        this.modalService.setError('رقم الهاتف يجب أن يكون 10 أرقام فقط');
        return;
      }

      // Create provider data
      const providerData = {
        providerName: formData.providerName.trim(),
        password: formData.password,
        confirmPassword: formData.confirmPassword,
        mobile: formData.mobile.trim(),
        telephone: formData.telephone?.trim() || '',
        address: formData.address.trim()
      };

      const success = await this.providerService.createProvider(providerData).toPromise();

      if (success) {
        this.modalService.setResult({ action: 'save', data: formData });
        this.modalComponent.hide();
        
        // Refresh data
        await this.loadProviders();
        await this.loadStatistics();
        
        // Show success message
        // this.errorPopup.showSuccess('تم إنشاء حساب المزود بنجاح');
      } else {
        this.modalService.setError('فشل في إنشاء الحساب. حاول مرة أخرى.');
      }

    } catch (error: any) {
      console.error('Error creating provider:', error);
      
      let errorMessage = 'حدث خطأ أثناء إنشاء الحساب';
      
      if (error?.error?.message) {
        errorMessage = error.error.message;
      } else if (error?.message) {
        errorMessage = error.message;
      } else if (typeof error === 'string') {
        errorMessage = error;
      }

      this.modalService.setError(errorMessage);
    } finally {
      this.modalService.setSaving(false);
    }
    this.isProcessingModal = false;
  }

  onModalCancel(): void {
    this.modalComponent.hide();
    this.modalService.setResult({ action: 'cancel' });
  }

  async loadProviders(): Promise<void> {
    if (this.isLoadingProviders) return;
    
    this.isLoadingProviders = true;
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
        this.currentPage = response.pagination.currentPage;
        this.totalPages = response.pagination.totalPages;
        this.totalCount = response.pagination.totalCount;
      }
    } catch (error: any) {
      this.error = 'حدث خطأ أثناء تحميل بيانات المزودين';
      console.error('Error loading providers:', error);
    } finally {
      this.loading = false;
      this.isLoadingProviders = false;
    }
  }

  async loadStatistics(): Promise<void> {
    if (this.isLoadingStatistics) return;
    
    this.isLoadingStatistics = true;

    try {
      this.statistics = await this.providerService.getProviderStatistics().toPromise();
    } catch (error: any) {
      console.error('Error loading statistics:', error);
    } finally {
      this.isLoadingStatistics = false;
    }
  }

  // Search functionality
  onSearchChange(): void {
    this.searchSubject.next(this.searchTerm);
  }

  clearSearch(): void {
    this.searchTerm = '';
    this.onSearchChange();
  }

  // Filter functionality
  onStatusChange(): void {
    this.currentPage = 1;
    this.loadProviders();
  }

  onVerificationChange(): void {
    this.currentPage = 1;
    this.loadProviders();
  }

  toggleFilters(): void {
    this.showFilters = !this.showFilters;
  }

  clearFilters(): void {
    this.selectedStatus = '';
    this.selectedVerification = '';
    this.currentPage = 1;
    this.loadProviders();
  }

  // Sorting functionality
  sort(field: string): void {
    if (this.sortBy === field) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortBy = field;
      this.sortDirection = 'asc';
    }
    this.currentPage = 1;
    this.loadProviders();
  }

  getSortIcon(field: string): string {
    if (this.sortBy !== field) {
      return 'fas fa-sort text-muted';
    }
    return this.sortDirection === 'asc' ? 'fas fa-sort-up text-primary' : 'fas fa-sort-down text-primary';
  }

  // Pagination functionality
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
      this.currentPage = page;
      this.loadProviders();
    }
  }

  get pageNumbers(): number[] {
    const pages: number[] = [];
    const start = Math.max(1, this.currentPage - 2);
    const end = Math.min(this.totalPages, this.currentPage + 2);
    
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    
    return pages;
  }

  // Provider status methods
  getProviderStatus(provider: ProviderManagementDto): string {
    return this.providerService.getProviderStatus(provider);
  }

  getStatusBadgeClass(provider: ProviderManagementDto): string {
    return this.providerService.getStatusBadgeClass(provider);
  }

  getStatusIcon(provider: ProviderManagementDto): string {
    return this.providerService.getStatusIcon(provider);
  }

  isProviderLocked(provider: ProviderManagementDto): boolean {
    return this.providerService.isProviderLocked(provider);
  }

  // Provider actions
  viewProvider(provider: ProviderManagementDto): void {
    this.selectedProvider = provider;
    // Implement view logic
  }

  editProvider(provider: ProviderManagementDto): void {
    this.selectedProvider = provider;
    // Implement edit logic
  }

  openLockModal(provider: ProviderManagementDto): void {
    this.selectedProvider = provider;
    this.lockReason = '';
    this.lockUntilDate = '';
    
    if (this.lockModal) {
      this.lockModal.show();
    }
  }

  async lockProvider(): Promise<void> {
    if (!this.selectedProvider) return;

    this.lockingProvider = true;

    try {
      const lockUntil = this.lockUntilDate ? new Date(this.lockUntilDate) : undefined;
      const success = await this.providerService.lockProvider(
        this.selectedProvider.tenantId, 
        lockUntil, 
        this.lockReason
      ).toPromise();

      if (success) {
        await this.loadProviders();
        await this.loadStatistics();
        this.lockModal.hide();
        // this.errorPopup.showSuccess('تم قفل الحساب بنجاح');
      } else {
        this.errorPopup.showError('فشل في قفل الحساب');
      }
    } catch (error: any) {
      this.errorPopup.showError('حدث خطأ أثناء قفل الحساب');
      console.error('Error locking provider:', error);
    } finally {
      this.lockingProvider = false;
    }
  }

  async unlockProvider(provider: ProviderManagementDto): Promise<void> {
    const actionKey = `unlock_${provider.id}`;
    this.actionLoading[actionKey] = true;

    try {
      const success = await this.providerService.unlockProvider(provider.tenantId).toPromise();

      if (success) {
        await this.loadProviders();
        await this.loadStatistics();
        // this.errorPopup.showSuccess('تم إلغاء قفل الحساب بنجاح');
      } else {
        this.errorPopup.showError('فشل في إلغاء قفل الحساب');
      }
    } catch (error: any) {
      this.errorPopup.showError('حدث خطأ أثناء إلغاء قفل الحساب');
      console.error('Error unlocking provider:', error);
    } finally {
      this.actionLoading[actionKey] = false;
    }
  }

  async updateVerification(provider: ProviderManagementDto, isEmailVerified: boolean, isPhoneVerified: boolean): Promise<void> {
    const actionKey = `verify_${provider.id}`;
    this.actionLoading[actionKey] = true;

    try {
      const request = { isEmailVerified, isPhoneVerified };
      const success = await this.providerService.updateProviderVerification(provider.tenantId, request).toPromise();

      if (success) {
        await this.loadProviders();
        await this.loadStatistics();
        // this.errorPopup.showSuccess('تم تحديث حالة التحقق بنجاح');
      } else {
        this.errorPopup.showError('فشل في تحديث حالة التحقق');
      }
    } catch (error: any) {
      this.errorPopup.showError('حدث خطأ أثناء تحديث حالة التحقق');
      console.error('Error updating verification:', error);
    } finally {
      this.actionLoading[actionKey] = false;
    }
  }

  // Utility methods
  formatDate(date: Date | string | null): string {
    if (!date) return 'غير متوفر';
    
    const d = typeof date === 'string' ? new Date(date) : date;
  return d.toLocaleDateString('ar-SA-u-ca-gregory', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
    });
  }

formatMobile(mobile: string): string {
  if (!mobile) return '';

  // Format mobile number (e.g., 0901234567 -> 090 123 4567)
  const formatted = mobile.replace(/(\d{3})(\d{3})(\d{4})/, '$1 $2 $3');

  // Convert to Arabic-Indic numerals
  const arabicFormatted = formatted.replace(/\d/g, d =>
    '٠١٢٣٤٥٦٧٨٩'[parseInt(d)]
  );

  return arabicFormatted;
}
}

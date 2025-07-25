import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../enviroments/environment.development';
import { HttpClient, HttpParams } from '@angular/common/http';

export interface ProviderManagementDto {
  id: string;
  tenantId: string;
  providerName: string;
  telephone: string;
  mobile: string;
  address?:string;
  creationTime: Date;
  email?: string;
  displayName?: string;
  accountStatus: string;
  isEmailVerified: boolean;
  isPhoneVerified: boolean;
  lastLoginDate?: Date;
  lockoutEnd?: Date;
  lockoutEnabled: boolean;
  accessFailedCount: number;
}

export interface PaginationInfo {
  currentPage: number;
  totalPages: number;
  pageSize: number;
  totalCount: number;
  searchTerm?: string;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
  startItem: number;
  endItem: number;
}

export interface GetProvidersResponseDto {
  providers: ProviderManagementDto[];
  pagination: PaginationInfo;
}

export interface GetProvidersRequestDto {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  accountStatus?: string;
  isVerified?: boolean;
  sortBy?: string;
  sortDirection?: string;
}

export interface ProviderStatisticsDto {
  totalProviders: number;
  activeProviders: number;
  inactiveProviders: number;
  suspendedProviders: number;
  pendingVerificationProviders: number;
  registeredThisMonth: number;
  registeredToday: number;
}

export interface UpdateVerificationRequestDto {
  isEmailVerified: boolean;
  isPhoneVerified: boolean;
}

export interface LockAccountRequestDto {
  lockUntil?: any;
  reason?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ProviderManagementService {
   private apiUrl = environment.API_URL + "api/ProviderManagement/";

  constructor(private http: HttpClient) { }

  /**
   * Get all providers with advanced filtering and pagination
   * @param request Filter and pagination parameters
   * @returns Observable of GetProvidersResponseDto
   */
  getAllProviders(request: GetProvidersRequestDto = {}): Observable<GetProvidersResponseDto> {
    let params = new HttpParams();
    
    if (request.pageNumber) {
      params = params.set('pageNumber', request.pageNumber.toString());
    }
    if (request.pageSize) {
      params = params.set('pageSize', request.pageSize.toString());
    }
    if (request.searchTerm && request.searchTerm.trim()) {
      params = params.set('searchTerm', request.searchTerm.trim());
    }
    if (request.accountStatus) {
      params = params.set('accountStatus', request.accountStatus);
    }
    if (request.isVerified !== undefined) {
      params = params.set('isVerified', request.isVerified.toString());
    }
    if (request.sortBy) {
      params = params.set('sortBy', request.sortBy);
    }
    if (request.sortDirection) {
      params = params.set('sortDirection', request.sortDirection);
    }

    return this.http.get<GetProvidersResponseDto>(
      this.apiUrl + 'providers',
      { params }
    );
  }

  /**
   * Get a specific provider by ID
   * @param providerId Provider unique identifier
   * @returns Observable of ProviderManagementDto
   */
  getProviderById(providerId: string): Observable<ProviderManagementDto> {
    return this.http.get<ProviderManagementDto>(
      this.apiUrl + `providers/${providerId}`
    );
  }

  /**
   * Get a specific provider by tenant ID
   * @param tenantId Tenant unique identifier
   * @returns Observable of ProviderManagementDto
   */
  getProviderByTenantId(tenantId: string): Observable<ProviderManagementDto> {
    return this.http.get<ProviderManagementDto>(
      this.apiUrl + `providers/tenant/${tenantId}`
    );
  }

  /**
   * Update provider verification status
   * @param tenantId Tenant ID of the provider
   * @param request Verification update request
   * @returns Observable of boolean
   */
  updateProviderVerification(
    tenantId: string, 
    request: UpdateVerificationRequestDto
  ): Observable<boolean> {
    return this.http.put<boolean>(
      this.apiUrl + `providers/${tenantId}/verification`,
      request
    );
  }

  /**
   * Lock or unlock a provider account
   * @param tenantId Tenant ID of the provider
   * @param request Lock account request
   * @returns Observable of boolean
   */
  lockProviderAccount(
    tenantId: string, 
    request: LockAccountRequestDto
  ): Observable<boolean> {
    return this.http.put<boolean>(
      this.apiUrl + `providers/${tenantId}/lock`,
      request
    );
  }

  /**
   * Lock a provider account until specified date
   * @param tenantId Tenant ID of the provider
   * @param lockUntil Date to lock until
   * @param reason Reason for locking
   * @returns Observable of boolean
   */
  lockProvider(tenantId: string, lockUntil?: Date, reason?: string): Observable<boolean> {
    const request: LockAccountRequestDto = {
      lockUntil: lockUntil,
      reason: reason
    };
    return this.lockProviderAccount(tenantId, request);
  }

  /**
   * Unlock a provider account
   * @param tenantId Tenant ID of the provider
   * @returns Observable of boolean
   */
  unlockProvider(tenantId: string): Observable<boolean> {
    const request: LockAccountRequestDto = {
      lockUntil: null
    };
    return this.lockProviderAccount(tenantId, request);
  }

  /**
   * Get provider statistics for dashboard
   * @returns Observable of ProviderStatisticsDto
   */
  getProviderStatistics(): Observable<ProviderStatisticsDto> {
    return this.http.get<ProviderStatisticsDto>(
      this.apiUrl + 'statistics'
    );
  }

  /**
   * Check if provider is currently locked
   * @param provider Provider object
   * @returns Boolean indicating if provider is locked
   */
  isProviderLocked(provider: ProviderManagementDto): boolean {
    if (!provider.lockoutEnd) {
      return false;
    }
    
    const lockoutDate = new Date(provider.lockoutEnd);
    const now = new Date();
    
    return lockoutDate > now;
  }

  /**
   * Get provider status with lock information
   * @param provider Provider object
   * @returns Status string
   */
  getProviderStatus(provider: ProviderManagementDto): string {
    if (this.isProviderLocked(provider)) {
      return 'مقفل';
    }

    switch (provider.accountStatus.toLowerCase()) {
      case 'نشط':
      case 'active':
        return 'نشط';
      case 'بانتظار التنشيط':
      case 'pending verification':
        return 'بانتظار التنشيط';
      case 'غير نشط':
      case 'inactive':
        return 'غير نشط';
      case 'معطل':
      case 'suspended':
        return 'معطل';
      default:
        return provider.accountStatus;
    }
  }

  /**
   * Helper method to get account status badge class
   * @param provider Provider object
   * @returns CSS class for status badge
   */
  getStatusBadgeClass(provider: ProviderManagementDto): string {
    if (this.isProviderLocked(provider)) {
      return 'badge-danger';
    }

    const status = provider.accountStatus.toLowerCase();
    switch (status) {
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

  /**
   * Helper method to get account status icon
   * @param provider Provider object
   * @returns FontAwesome icon class
   */
  getStatusIcon(provider: ProviderManagementDto): string {
    if (this.isProviderLocked(provider)) {
      return 'fas fa-lock';
    }

    const status = provider.accountStatus.toLowerCase();
    switch (status) {
      case 'نشط':
      case 'active':
        return 'fas fa-check-circle';
      case 'معطل':
      case 'suspended':
        return 'fas fa-ban';
      case 'بانتظار التنشيط':
      case 'pending verification':
        return 'fas fa-clock';
      case 'غير نشط':
      case 'inactive':
        return 'fas fa-times-circle';
      default:
        return 'fas fa-question-circle';
    }
  }
}

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
   * Get provider statistics for dashboard
   * @returns Observable of ProviderStatisticsDto
   */
  getProviderStatistics(): Observable<ProviderStatisticsDto> {
    return this.http.get<ProviderStatisticsDto>(
      this.apiUrl + 'statistics'
    );
  }

  /**
   * Helper method to get account status badge class
   * @param status Account status
   * @returns CSS class for status badge
   */
  getStatusBadgeClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'active':
        return 'badge-success';
      case 'suspended':
        return 'badge-danger';
      case 'pending verification':
        return 'badge-warning';
      case 'inactive':
        return 'badge-secondary';
      default:
        return 'badge-secondary';
    }
  }

  /**
   * Helper method to get account status icon
   * @param status Account status
   * @returns FontAwesome icon class
   */
  getStatusIcon(status: string): string {
    switch (status.toLowerCase()) {
      case 'active':
        return 'fas fa-check-circle';
      case 'suspended':
        return 'fas fa-ban';
      case 'pending verification':
        return 'fas fa-clock';
      case 'inactive':
        return 'fas fa-times-circle';
      default:
        return 'fas fa-question-circle';
    }
  }
}

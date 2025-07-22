import { TestBed } from '@angular/core/testing';

import { ProviderManagementService } from './provider-management-service';

describe('ProviderManagementService', () => {
  let service: ProviderManagementService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ProviderManagementService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

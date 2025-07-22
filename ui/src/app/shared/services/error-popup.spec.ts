import { TestBed } from '@angular/core/testing';

import { ErrorPopup } from './error-popup';

describe('ErrorPopup', () => {
  let service: ErrorPopup;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ErrorPopup);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

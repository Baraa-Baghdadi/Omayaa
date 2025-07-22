import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SharedPopup } from './shared-popup';

describe('SharedPopup', () => {
  let component: SharedPopup;
  let fixture: ComponentFixture<SharedPopup>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SharedPopup]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SharedPopup);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

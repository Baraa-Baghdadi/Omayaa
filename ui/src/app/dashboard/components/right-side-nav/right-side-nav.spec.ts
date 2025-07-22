import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RightSideNav } from './right-side-nav';

describe('RightSideNav', () => {
  let component: RightSideNav;
  let fixture: ComponentFixture<RightSideNav>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RightSideNav]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RightSideNav);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

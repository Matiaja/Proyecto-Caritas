import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CenterDetailComponent } from './center-detail.component';

describe('CenterDetailComponent', () => {
  let component: CenterDetailComponent;
  let fixture: ComponentFixture<CenterDetailComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CenterDetailComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(CenterDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

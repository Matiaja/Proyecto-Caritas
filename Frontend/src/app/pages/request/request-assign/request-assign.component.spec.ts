import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RequestAssignComponent } from './request-assign.component';

describe('RequestAssignComponent', () => {
  let component: RequestAssignComponent;
  let fixture: ComponentFixture<RequestAssignComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RequestAssignComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RequestAssignComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

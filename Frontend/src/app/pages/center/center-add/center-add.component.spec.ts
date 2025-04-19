import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CenterAddComponent } from './center-add.component';

describe('CenterAddComponent', () => {
  let component: CenterAddComponent;
  let fixture: ComponentFixture<CenterAddComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CenterAddComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(CenterAddComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

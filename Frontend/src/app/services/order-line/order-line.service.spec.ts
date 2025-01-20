import { TestBed } from '@angular/core/testing';

import { OrderLineService } from './order-line.service';

describe('OrderLineService', () => {
  let service: OrderLineService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(OrderLineService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

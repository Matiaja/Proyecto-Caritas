import { TestBed } from '@angular/core/testing';

import { DistributionService } from './distribution.service';

describe('DistributionService', () => {
  let service: DistributionService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DistributionService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

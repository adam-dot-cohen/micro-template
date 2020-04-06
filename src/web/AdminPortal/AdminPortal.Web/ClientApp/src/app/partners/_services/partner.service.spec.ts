import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';

import { PartnerService } from './partner.service';

describe('PartnerService', () => {
  beforeEach(() => TestBed.configureTestingModule({
    imports: [HttpClientTestingModule],
  }));

  it('should be created', () => {
    const service: PartnerService = TestBed.get(PartnerService);
    expect(service).toBeTruthy();
  });
});

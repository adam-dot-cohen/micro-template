import { TestBed, inject } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';

import { AuthorizeService } from './authorize.service';

describe('AuthorizeService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ AuthorizeService ],
      imports: [ RouterTestingModule ]
    });
  });

  it('should be created',
    inject([AuthorizeService], (service: AuthorizeService) => {
    expect(service).toBeTruthy();
  }));
});

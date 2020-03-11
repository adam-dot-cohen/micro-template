import { TestBed } from '@angular/core/testing';
import { RouterModule, Router } from '@angular/router';

import { AuthorizeService } from './authorize.service';

describe('AuthorizeService', () => {
  beforeEach(() => TestBed.configureTestingModule({
    imports: [ Router, RouterModule.forRoot([]) ]
  }));

  it('should be created', () => {
    const service: AuthorizeService = TestBed.get(AuthorizeService);
    expect(service).toBeTruthy();
  });
});

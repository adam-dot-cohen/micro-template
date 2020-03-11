import { TestBed , inject } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';

import { AuthorizeInterceptor } from '@app/api-auth/authorize.interceptor';

describe('AuthorizeService', () => {
  beforeEach(() => TestBed.configureTestingModule({
    providers: [ AuthorizeInterceptor ],
    imports: [ RouterTestingModule ]
  }));

  it('should be created',
    inject([ AuthorizeInterceptor ], (interceptor: AuthorizeInterceptor) => {
    expect(interceptor).toBeTruthy();
  }));
});

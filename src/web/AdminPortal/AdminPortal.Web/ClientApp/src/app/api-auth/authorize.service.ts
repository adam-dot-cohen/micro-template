import { Injectable } from '@angular/core';
import { Location } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class AuthorizeService {

  constructor(private readonly location: Location) { }

  public login(): void {
    const authPath = this.location.prepareExternalUrl(`/auth/login?returnUrl=${this.location.path()}`);
    const loginUrl = `${document.location.origin}${authPath}`;

    document.location.href = loginUrl;
  }

  public logout(): void {
    const authPath = this.location.prepareExternalUrl('/auth/logout');
    const logoutUrl = `${document.location.origin}${authPath}`;

    document.location.href = logoutUrl;
  }
}

import { Injectable } from '@angular/core';
import { Resolve, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';

import { PartnerConfiguration } from '../_models/partnerconfiguration';
import { PartnerService } from '../_services/partner.service';

@Injectable({ providedIn: 'root' })
export class PartnerConfigurationResolver implements Resolve<PartnerConfiguration> {
  constructor(private readonly service: PartnerService) {}

  public resolve(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<any> | Promise<any> | any {
    return this.service.getPartnerConfiguration(route.paramMap.get('id'));
  }
}

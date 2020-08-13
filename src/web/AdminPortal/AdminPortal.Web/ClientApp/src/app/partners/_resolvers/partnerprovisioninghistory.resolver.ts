import { Injectable } from '@angular/core';
import { Resolve, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';

import { PartnerProvisioningHistory } from '../_models/partner-provisioning-history';
import { PartnerService } from '../_services/partner.service';

@Injectable({ providedIn: 'root' })
export class PartnerProvisioningHistoryResolver implements Resolve<PartnerProvisioningHistory> {
  constructor(private readonly service: PartnerService) {}

  public resolve(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<any> | Promise<any> | any {
    return this.service.getPartnerProvisioningHistory(route.paramMap.get('id'));
  }
}

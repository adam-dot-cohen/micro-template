import { Injectable } from '@angular/core';
import { Resolve, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';

import { Partner } from '../_models/partner';
import { PartnerService } from '../_services/partner.service';

@Injectable({ providedIn: 'root' })
export class PartnerResolver implements Resolve<Partner> {
  constructor(private readonly service: PartnerService) {}

  public resolve(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<any> | Promise<any> | any {
    console.log(route.paramMap.get('id'));
    return this.service.getPartner(route.paramMap.get('id'));
  }
}

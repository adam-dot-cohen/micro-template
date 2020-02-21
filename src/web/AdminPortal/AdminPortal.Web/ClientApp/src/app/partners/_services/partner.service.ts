import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Partner } from '../_models/partner';
import { environment } from '@env/environment';

@Injectable({
  providedIn: 'root'
})
export class PartnerService {
  constructor(private http: HttpClient) { }

  getPartners(): Observable<Partner[]> {
    return this.http.get<Partner[]>(environment.partnerApiUrl);
  }

  createPartner(partner: Partner): void {
    console.log(`POST to partner url: ${environment.partnerApiUrl}`);
    console.log(partner);
    // this.http.post(environment.partnerApiUrl, partner);
  }
}

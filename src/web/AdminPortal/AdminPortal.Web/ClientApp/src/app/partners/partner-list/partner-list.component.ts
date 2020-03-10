import { Component, OnInit } from '@angular/core';
import { BehaviorSubject, of } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';

import { PartnerService } from '@app/partners/_services/partner.service';
import { Partner } from '@app/partners/_models/partner';

@Component({
  templateUrl: './partner-list.component.html',
  styleUrls: ['./partner-list.component.scss']
})

export class PartnerListComponent implements OnInit {
  private loadingSubject = new BehaviorSubject<boolean>(false);

  constructor(private readonly partnerService: PartnerService) { }

  public loading$ = this.loadingSubject.asObservable();
  public partners: Partner[] = [];

  public ngOnInit() {
    this.loadPartners();
  }

  private loadPartners() {
    this.loadingSubject.next(true);

    this.partnerService.getPartners()
      .pipe(
        catchError(() => of([])),
        finalize(() => this.loadingSubject.next(false))
      )
      .subscribe({
        next: result => this.partners = ((result) as any)
      });
  }
}

import { CollectionViewer, DataSource } from '@angular/cdk/collections';
import { Observable, BehaviorSubject, of } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';

import { Partner } from '@app/partners/_models/partner';
import { PartnerService } from '@app/partners/_services/partner.service';

export class PartnersDataSource implements DataSource<Partner> {

  private partnersSubject = new BehaviorSubject<Partner[]>([]);
  private loadingSubject = new BehaviorSubject<boolean>(false);

  constructor(private readonly partnerService: PartnerService) {}

  public loading$ = this.loadingSubject.asObservable();

  public connect(collectionViewer: CollectionViewer): Observable<Partner[]> {
    return this.partnersSubject.asObservable();
  }

  public disconnect(collectionViewer: CollectionViewer): void {
    this.partnersSubject.complete();
    this.loadingSubject.complete();
  }

  public loadPartners() {
    this.loadingSubject.next(true);

    this.partnerService.getPartners().pipe(
        catchError(() => of([])),
        finalize(() => this.loadingSubject.next(false))
      )
      .subscribe(partners => this.partnersSubject.next(partners));
  }
}

import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '@env/environment';

import { Partner } from '@app/partners//_models/partner';
import { PartnerConfiguration } from '@app/partners/_models/partnerconfiguration';

@Injectable({
  providedIn: 'root'
})
export class PartnerService {
  constructor(private readonly http: HttpClient) {
  }

  public getPartners(): Observable<Partner[]> {
    return this.http.get<Partner[]>(environment.partnerApiUrl)
      .pipe(catchError(this.handleError)
    );
  }

  public getPartner(partnerId: string): Observable<Partner> {
    return this.http.get<Partner>(`${environment.partnerApiUrl}/${partnerId}`)
      .pipe(catchError(this.handleError));
  }

  public createPartner(partner: Partner): Observable<Partner> {
    return this.http.post<Partner>(environment.partnerApiUrl, partner)
      .pipe(catchError(this.handleError)
    );
  }

  public getPartnerConfiguration(partnerId: string): Observable<PartnerConfiguration> {
    return this.http.get<PartnerConfiguration>(`${environment.partnerApiUrl}/${partnerId}/configuration`)
      .pipe(catchError(this.handleError));
  }

  // Consider HttpInterceptor for error handling
  private handleError(errorResponse: HttpErrorResponse) {
    // TODO: Send errors to logging
    let errorMessage = '';
    const errorResult = (errorResponse.error && errorResponse.error.message) ? errorResponse.error as ErrorResult : undefined;

    if (errorResponse.error instanceof ErrorEvent) {
      // A client-side or network error occurred. Handle it accordingly.
      errorMessage = `An error occurred: ${errorResponse.error.message}`;
      console.error('An error occurred:', errorResponse.error.message);
    } else if (errorResult) {
      // An application error occurred.
      errorMessage = errorResult.message;
      if (errorResult.validationMessages && errorResult.validationMessages.length) {
        errorMessage += ' - ' + errorResult.validationMessages.map(m => `${m.key}: ${m.message}`).reduce((acc, m) => acc += m);
      }
    } else {
      // The backend returned an unsuccessful response code.
      // The response body may contain clues as to what went wrong,
      errorMessage = `Server returned code: ${errorResponse.status}, error message is: ${errorResponse.message}`;
      console.error(
        `Backend returned code ${errorResponse.status}, ` +
        `body was: ${errorResponse.error}`);
      console.log(errorResponse);
    }

    // return an observable with a user-facing error message
    return throwError(errorMessage);
  }
}

class ErrorResult {
  public message: string;
  public validationMessages: ValidationMessage[];
}

class ValidationMessage {
  public key: string;
  public message: string;
}

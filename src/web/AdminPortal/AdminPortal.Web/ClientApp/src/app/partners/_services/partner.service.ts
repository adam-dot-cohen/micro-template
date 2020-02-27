import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Partner } from '../_models/partner';
import { environment } from '@env/environment';

@Injectable({
  providedIn: 'root'
})
export class PartnerService {
  constructor(private http: HttpClient) { }

  getPartners(): Observable<Partner[]> {
    return this.http.get<Partner[]>(environment.partnerApiUrl)
      .pipe(
        catchError(this.handleError)
    );
  }

  createPartner(partner: Partner): Observable<Partner> {
    console.log(`POST to partner url: ${environment.partnerApiUrl}`);
    console.log(partner);

    return this.http.post<Partner>(environment.partnerApiUrl, partner)
      .pipe(
        catchError(this.handleError)
    );
  }

  // Consider HttpInterceptor for error handling
  private handleError(errorResponse: HttpErrorResponse) {
    // TODO: Send errors to Loggly
    let errorMessage = '';
    if (errorResponse.error instanceof ErrorEvent) {
      // A client-side or network error occurred. Handle it accordingly.
      errorMessage = `An error occurred: ${errorResponse.error.message}`;
      console.error('An error occurred:', errorResponse.error.message);
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
  };
}

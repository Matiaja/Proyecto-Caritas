import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { DonationRequest } from '../../models/donationRequest.model';

@Injectable({
  providedIn: 'root'
})
export class DonationRequestService {

  baseUrl = environment.baseUrl;
  constructor(private http: HttpClient) {}

  addDonationRequest(dr: DonationRequest): Observable<DonationRequest> {
    return this.http.post<DonationRequest>(this.baseUrl + 'donationRequests', dr);
  }
}

import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { Movement } from '../../models/movement.model';

@Injectable({
  providedIn: 'root'
})
export class MovementService {

  baseUrl = environment.baseUrl;

  constructor(private http: HttpClient) {}

  getDonationMovements(filters?: {
    dateFrom?: string;
    dateTo?: string;
    status?: string;
    productName?: string;
    centerId?: number;
    typeCenter?: string;
  }): Observable<Movement[]> {
    let params = new HttpParams();
    if (filters?.dateFrom) params = params.set('dateFrom', filters.dateFrom);
    if (filters?.dateTo) params = params.set('dateTo', filters.dateTo);
    if (filters?.status) params = params.set('status', filters.status);
    if (filters?.productName != null) params = params.set('productName', filters.productName);
    if (filters?.centerId != null) params = params.set('centerId', filters.centerId.toString());
    if (filters?.centerId != null && filters?.typeCenter) params = params.set('typeCenter', filters.typeCenter);

    return this.http.get<Movement[]>(`${this.baseUrl}donationRequests/movements`, { params });
  }

  getDistributionMovements(filters?: {
    dateFrom?: string;
    dateTo?: string;
    status?: string;
    productName?: string;
    centerId?: number;
    typeCenter?: string;
  }): Observable<Movement[]> {
    let params = new HttpParams();
    if (filters?.dateFrom) params = params.set('dateFrom', filters.dateFrom);
    if (filters?.dateTo) params = params.set('dateTo', filters.dateTo);
    if (filters?.status) params = params.set('status', filters.status);
    if (filters?.productName != null) params = params.set('productName', filters.productName);
    if (filters?.centerId != null) params = params.set('centerId', filters.centerId.toString());
    if (filters?.centerId != null && filters?.typeCenter) params = params.set('typeCenter', filters.typeCenter);

    return this.http.get<Movement[]>(`${this.baseUrl}distributions/movements-centers`, { params });
  }

  /* getDirectMovements(filters?: {
    ddateFrom?: string;
    dateTo?: string;
    status?: string;
    productName?: string;
    centerId?: number;
    typeCenter?: string;
  }): Observable<Movement[]> {
    let params = new HttpParams();
    if (filters?.dateFrom) params = params.set('dateFrom', filters.dateFrom);
    if (filters?.dateTo) params = params.set('dateTo', filters.dateTo);
    if (filters?.status) params = params.set('status', filters.status);
    if (filters?.productName != null) params = params.set('productName', filters.productName);
    if (filters?.centerId != null) params = params.set('centerId', filters.centerId.toString());
    if (filters?.centerId != null && filters?.typeCenter) params = params.set('typeCenter', filters.typeCenter);

    return this.http.get<Movement[]>(`${this.baseUrl}directMovements`, { params });
  } */
}

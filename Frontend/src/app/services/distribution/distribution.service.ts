import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DistributionService {
  private baseUrl = environment.baseUrl + 'distributions';
  constructor(private http: HttpClient) {}

  create(distribution: any): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}`, distribution);
  }
}

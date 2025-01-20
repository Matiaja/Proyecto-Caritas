import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import { RequestModel } from '../models/request.model';

@Injectable({
  providedIn: 'root'
})
export class RequestService {

  baseUrl = environment.baseUrl;
  constructor(private http: HttpClient) { }

  getRequests(): Observable<RequestModel[]> {
    return this.http.get<RequestModel[]>(this.baseUrl + 'requests');
  }

  getRequestsByCenter(centerId: number): Observable<RequestModel[]> {
    return this.http.get<RequestModel[]>(this.baseUrl + 'requests/center/' + centerId);
  }

  getRequestById(requestId: number): Observable<RequestModel> {
    return this.http.get<RequestModel>(this.baseUrl + 'requests/' + requestId);
  }
}

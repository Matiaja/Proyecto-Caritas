import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { RequestModel } from '../../models/request.model';

@Injectable({
  providedIn: 'root',
})
export class RequestService {
  baseUrl = environment.baseUrl;
  constructor(private http: HttpClient) {}

  getRequests(): Observable<RequestModel[]> {
    return this.http.get<RequestModel[]>(this.baseUrl + 'requests');
  }

  getRequestsByCenter(centerId: number): Observable<RequestModel[]> {
    return this.http.get<RequestModel[]>(this.baseUrl + 'requests/center/' + centerId);
  }

  getRequestById(requestId: number): Observable<RequestModel> {
    return this.http.get<RequestModel>(this.baseUrl + 'requests/' + requestId);
  }

  addRequest(request: RequestModel): Observable<RequestModel> {
    return this.http.post<RequestModel>(this.baseUrl + 'requests', request);
  }

  closeRequest(requestId: number): Observable<any> {
    return this.http.put(this.baseUrl + 'requests/' + requestId + '/close', null);
  }
}

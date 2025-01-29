import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { CenterModel } from '../../models/center.model';

@Injectable({
  providedIn: 'root'
})
export class CenterService {
  baseUrl = environment.baseUrl;

  constructor(private http: HttpClient) {}

  getCenters(): Observable<CenterModel[]> {
    return this.http.get<CenterModel[]>(this.baseUrl + 'centers');
  }
  getCenter(id: number): Observable<any> {
    const token = localStorage.getItem('authUser');
    const headers = token
      ? new HttpHeaders().set('Authorization', `Bearer ${token}`)
      : undefined;
    return this.http.get<any>(`${this.baseUrl}centers/${id}`, { headers }); 
  }
  getCenterById(centerId: number): Observable<CenterModel> {
    return this.http.get<CenterModel>(`${this.baseUrl}centers/${centerId}`);
  }
  
  createCenter(center: CenterModel): Observable<CenterModel> {
    return this.http.post<CenterModel>(this.baseUrl + 'centers', center);
  }

  updateCenter(id: number, center: any): Observable<any> {
    const token = localStorage.getItem('authUser');
    const headers = token
      ? new HttpHeaders().set('Authorization', `Bearer ${token}`)
      : undefined;
    return this.http.put(`${this.baseUrl}centers/${id}`, center, { headers });
  }
  
  deleteCenter(centerId: number): Observable<void> {
    return this.http.delete<void>(this.baseUrl + 'centers/' + centerId);
  }
}

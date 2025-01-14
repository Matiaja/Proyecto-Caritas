import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
    
  private baseUrl = environment.baseUrl + 'categories';
  constructor(private http: HttpClient) {}
  getCategories(): Observable<any[]> {
    const token = localStorage.getItem('authUser');
    const headers = token
      ? new HttpHeaders().set('Authorization', `Bearer ${token}`)
      : undefined;
      return this.http.get<any[]>(this.baseUrl, { headers });
    }
}

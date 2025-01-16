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

    getCategory(id: number): Observable<any> {
      const token = localStorage.getItem('authUser');
      const headers = token
        ? new HttpHeaders().set('Authorization', `Bearer ${token}`)
        : undefined;
        return this.http.get<any>(`${this.baseUrl}/${id}`, { headers });
    }

    createCategory(category: any): Observable<any> {
      const token = localStorage.getItem('authUser');
      const headers = token
        ? new HttpHeaders().set('Authorization', `Bearer ${token}`)
        : undefined;
        return this.http.post(this.baseUrl, category, { headers });
    }

    updateCategory(id: number, category: any): Observable<any> {
      const token = localStorage.getItem('authUser');
      const headers = token
        ? new HttpHeaders().set('Authorization', `Bearer ${token}`)
        : undefined;
        return this.http.put(`${this.baseUrl}/${id}`, category, { headers });
    }

    deleteCategory(id: number): Observable<any> {
      const token = localStorage.getItem('authUser');
      const headers = token
        ? new HttpHeaders().set('Authorization', `Bearer ${token}`)
        : undefined;
        return this.http.delete(`${this.baseUrl}/${id}`, { headers });
    }

    
}

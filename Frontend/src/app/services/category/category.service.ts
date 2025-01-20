import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
    
  private baseUrl = environment.baseUrl + 'categories';
  private categoriesSubject = new BehaviorSubject<any[]>([]);
  categories$ = this.categoriesSubject.asObservable();

  constructor(private http: HttpClient) {}
  getCategories() {
    this.http.get<any[]>(this.baseUrl).subscribe((categories) => {
      this.categoriesSubject.next(categories);
    });
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

    deleteCategory(id: number) {
      return this.http.delete(`${this.baseUrl}/${id}`).pipe(
        tap(() => {
          const currentCategories = this.categoriesSubject.getValue();
          const updatedCategories = currentCategories.filter((c) => c.id !== id);
          this.categoriesSubject.next(updatedCategories);
        })
      );
    }

    
}

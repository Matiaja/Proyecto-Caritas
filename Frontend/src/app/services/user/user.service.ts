import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable, BehaviorSubject, tap, catchError, of } from 'rxjs';
import { HttpParams } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  baseUrl = environment.baseUrl + 'user';
  private usersSubject = new BehaviorSubject<any[]>([]);
  users$ = this.usersSubject.asObservable();

  constructor(private http: HttpClient) {}

  getUsers(): Observable<any[]> {
    return this.http.get<any[]>(this.baseUrl).pipe(
      tap((users) => {
        this.usersSubject.next(users);
      })
    );
  }

  getUsersNoAdmin(): void {
    this.http
      .get<any[]>(this.baseUrl + '/all-user-no-admin')
      .pipe(
        tap((users) => {
          this.usersSubject.next(users);
        }),
        catchError((error) => {
          console.error('Error obteniendo usuarios:', error);
          return of([]);
        })
      )
      .subscribe();
  }

  getUserById(userId: string): Observable<any> {
    return this.http.get<any>(this.baseUrl + '/' + userId);
  }

  deleteUser(userId: string) {
    return this.http.delete(this.baseUrl + '/' + userId).pipe(
      tap(() => {
        const currentUsers = this.usersSubject.getValue();
        const updatedUsers = currentUsers.filter((u) => u.id !== userId);
        this.usersSubject.next(updatedUsers);
      })
    );
  }

  getFilteredUsers(centerId?: number, sortBy?: string, order = 'asc'): void {
    let params = new HttpParams();
    if (centerId) {
      params = params.set('centerId', centerId);
    }
    if (sortBy) {
      params = params.set('sortBy', sortBy);
      params = params.set('order', order);
    }
    this.http.get<any[]>(`${this.baseUrl}/filter`, { params }).subscribe((users) => {
      this.usersSubject.next(users);
    });
  }

  updateUser(user: any, userId: string): Observable<any> {
    return this.http.put(this.baseUrl + '/' + userId, user);
  }

  registerUser(user: any): Observable<any> {
    return this.http.post(this.baseUrl + '/register', user);
  }
}

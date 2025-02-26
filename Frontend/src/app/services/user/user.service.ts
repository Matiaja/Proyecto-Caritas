import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Product } from '../../models/product.model';
import { Observable, BehaviorSubject, tap, map, catchError, of} from 'rxjs';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  baseUrl = environment.baseUrl + 'user';
  private usersSubject = new BehaviorSubject<any[]>([]);
  users$ = this.usersSubject.asObservable();

  constructor(
    private http: HttpClient,
  ) { }

  getUsers(): Observable<any[]> {
    return this.http.get<any[]>(this.baseUrl).pipe(
      tap((users) => {
        this.usersSubject.next(users);
      })
    );
  }

  getUsersNoAdmin(): void {
    this.http.get<any[]>(this.baseUrl + '/all-user-no-admin').pipe(
      tap(users => {
        console.log("Usuarios recibidos:", users);
        this.usersSubject.next(users);
      }),
      catchError(error => {
        console.error("Error obteniendo usuarios:", error);
        return of([]); 
      })
    ).subscribe();
  }

  getUserById(userId: number): Observable<any> {
    return this.http.get<any>(this.baseUrl + '/' + userId);
  }

  deleteUser(userId: number){
    return this.http.delete(this.baseUrl + '/' + userId).pipe(
      tap(() => {
        const currentUsers = this.usersSubject.getValue();
        const updatedUsers = currentUsers.filter((u) => u.id !== userId);
        this.usersSubject.next(updatedUsers);
      })
    );
  }

  registerUser(user: any): Observable<any> {
    return this.http.post(this.baseUrl + '/register', user);
  }

}

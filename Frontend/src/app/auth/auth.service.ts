import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { inject } from '@angular/core';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  constructor() { }

  httpClient = inject(HttpClient);
  baseUrl = environment.baseUrl + 'user';

  signup(data: any) {
    return this.httpClient.post(`${this.baseUrl}/register`, data);
  }

  login(data: any) {
    return this.httpClient.post(`${this.baseUrl}/login`, data, {
      headers: { 'Content-Type': 'application/json' }
    }).pipe(
      tap((res: any) => {
        localStorage.setItem('authUser', JSON.stringify(res));
        localStorage.setItem('currentCenterId', res.centerId);
        console.log(res)
      })
    );
  }

  // login(data: any) {
  //   return this.httpClient.post(`${this.baseUrl}/login`, data).pipe(
  //     tap((res: any) => {
  //       localStorage.setItem('authUser', JSON.stringify(res));
  //     })
  //   );
  // }

  logout() {
    localStorage.removeItem('authUser');
  }

  isLoggedIn() {
    return localStorage.getItem('authUser') !== null;
  }

  getUserRole(): string | null {
    const token = localStorage.getItem('authUser');
    if (token) {
      const decoded: any = jwtDecode(token);
      const userRol = decoded.role;
      return userRol;
    }
    return null;
  }

}

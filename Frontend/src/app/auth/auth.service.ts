import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { inject } from '@angular/core';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  httpClient = inject(HttpClient);
  baseUrl = environment.baseUrl + 'user';

  signup(data: any) {
    return this.httpClient.post(`${this.baseUrl}/register`, data);
  }

  login(data: any) {
    return this.httpClient
      .post(`${this.baseUrl}/login`, data, {
        headers: { 'Content-Type': 'application/json' },
      })
      .pipe(
        tap((res: any) => {
          localStorage.setItem('authUser', JSON.stringify(res));
          localStorage.setItem('currentCenterId', res.centerId);
        })
      );
  }

  logout() {
    localStorage.removeItem('authUser');
  }

  isLoggedIn() {
    return localStorage.getItem('authUser') !== null;
  }

  getToken(): string {
    const authUser = localStorage.getItem('authUser');
    if (!authUser) return '';
    try {
      const parsed = JSON.parse(authUser);
      return parsed.token || '';
    } catch {
      return '';
    }
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

  getUserCenterId(): string | null {
    const token = localStorage.getItem('authUser');
    if (token) {
      const decoded: any = jwtDecode(token);
      return decoded.CenterId || null;
    }
    return null;
  }

  isAdmin(): boolean {
    const role = this.getUserRole();
    return role === 'Admin';
  }
}

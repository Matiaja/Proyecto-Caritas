import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CenterService {

  baseUrl = environment.baseUrl;
  constructor(private http: HttpClient) { }

  getCenters(): Observable<any[]> {
    return this.http.get<any[]>(this.baseUrl + 'centers');
  }
}

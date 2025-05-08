import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { OrderLine } from '../../models/orderLine.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class OrderLineService {
  baseUrl = environment.baseUrl;
  constructor(private http: HttpClient) {}

  getOrderLineById(orderLineId: number): Observable<OrderLine> {
    return this.http.get<OrderLine>(this.baseUrl + 'orderLines/' + orderLineId);
  }
}

import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable, forkJoin, map } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ProductStockSummary {
  productId: number;
  productName: string;
  productCode: string;
  categoryId: number;
  categoryName: string;
  centerId: number;
  centerName: string;
  totalStock: number;
  totalIngresos: number;
  totalEgresos: number;
  lastMovementDate: string;
  movementCount: number;
}

export interface StockHistory {
  stockId: number;
  stockDate: string;
  stockQuantity: number;
  stockType: string;
  centerId: number;
  productId: number;
  productName: string;
  categoryId: number;
  categoryName: string;
  stockAcumulado: number;
}

@Injectable({
  providedIn: 'root'
})
export class StockReportService {
  baseUrl = environment.baseUrl;
  constructor(private http: HttpClient) { }

  getStockReport(
    centerId?: number, 
    categoryId?: number, 
    productId?: number,
    fromDate?: string, 
    toDate?: string
  ): Observable<ProductStockSummary[]> {
    let params = new HttpParams();
    
    // Only add parameters if they have actual values
    if (centerId !== undefined && centerId !== null) {
      params = params.set('centerId', centerId.toString());
    }
    if (categoryId !== undefined && categoryId !== null) {
      params = params.set('categoryId', categoryId.toString());
    }
    if (productId !== undefined && productId !== null) {
      params = params.set('productId', productId.toString());
    }
    if (fromDate !== undefined && fromDate !== null && fromDate !== '') {
      params = params.set('dateFrom', fromDate);
    }
    if (toDate !== undefined && toDate !== null && toDate !== '') {
      params = params.set('dateTo', toDate);
    }

    return this.http.get<ProductStockSummary[]>(`${this.baseUrl}Stocks/filtered-stock`, { params });
  }

  getStockHistory(
    centerId?: number, 
    categoryId?: number, 
    productId?: number,
    fromDate?: string, 
    toDate?: string
  ): Observable<StockHistory[]> {
    let params = new HttpParams();
    
    // Only add parameters if they have actual values
    if (centerId !== undefined && centerId !== null) {
      params = params.set('centerId', centerId.toString());
    }
    if (categoryId !== undefined && categoryId !== null) {
      params = params.set('categoryId', categoryId.toString());
    }
    if (productId !== undefined && productId !== null) {
      params = params.set('productId', productId.toString());
    }
    if (fromDate !== undefined && fromDate !== null && fromDate !== '') {
      params = params.set('dateFrom', fromDate);
    }
    if (toDate !== undefined && toDate !== null && toDate !== '') {
      params = params.set('dateTo', toDate);
    }

    return this.http.get<StockHistory[]>(`${this.baseUrl}Stocks/stock-history`, { params });
  }
}


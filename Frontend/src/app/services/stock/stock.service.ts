import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable, BehaviorSubject, tap, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { HttpParams } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class StockService {
  baseUrl = environment.baseUrl + 'stocks';
  private stockSubject = new BehaviorSubject<any[]>([]);
  stocks$ = this.stockSubject.asObservable();
  totalItems = 0;

  constructor(private http: HttpClient) {}

  getStocks() {
    this.http.get<any[]>(this.baseUrl).subscribe((stocks) => {
      this.stockSubject.next(stocks);
    });
  }

  getStockById(stockId: number): Observable<any> {
    return this.http.get<any>(this.baseUrl + '/' + stockId);
  }

  createStock(stock: any): Observable<any> {
    return this.http.post<any>(this.baseUrl, stock).pipe(
      tap((newStock) => {
        const currentStocks = this.stockSubject.getValue();
        this.stockSubject.next([...currentStocks, newStock]);
      })
    );
  }

  deleteStock(stockId: number) {
    return this.http.delete(this.baseUrl + '/' + stockId).pipe(
      tap(() => {
        const currentStocks = this.stockSubject.getValue();
        const updatedStocks = currentStocks.filter((s) => s.id !== stockId);
        this.stockSubject.next(updatedStocks);
      })
    );
  }

  validateQuantity(centerId: number, productId: number, newQuantity: number, typeStock: string) {
    return this.http.get(`${this.baseUrl}/validate-quantity`, {
      params: { centerId, productId, newQuantity, type: typeStock},
    });
  }

  getProductWithStock(centerId: number | null, categoryId?: number, sortBy?: string, order = 'asc', groupByCenter?: boolean): void {
    let params = new HttpParams();
    if (categoryId) {
      params = params.set('categoryId', categoryId.toString());
    }
    if (sortBy) {
      params = params.set('sortBy', sortBy);
      params = params.set('order', order);
    }
    if (groupByCenter) {
      params = params.set('groupByCenter', groupByCenter.toString());
    }
    const headers = { centerId: centerId?.toString() ?? 'null' };
    this.http
      .get<any[]>(`${this.baseUrl}/product-with-stock`, { headers, params })
      .subscribe((products) => {
        this.stockSubject.next(products);
      });
  }

  getMoreQuantityProducts(centerId: number): Observable<any[]> {
    const headers = { centerId: centerId.toString() };
    return this.http.get<any[]>(`${this.baseUrl}/more-quantity-product-with-stock`, { headers });
  }

  getStockByCategory(centerId: number): Observable<any[]> {
    const headers = { centerId: centerId.toString() };
    return this.http.get<any[]>(`${this.baseUrl}/stock-by-category`, { headers });
  }

  getProductInStocks(productId: number, centerIdToAvoid: number): Observable<any[]> {
    let headers = new HttpHeaders();
    headers = headers.set('productId', productId.toString());
    headers = headers.set('centerIdToAvoid', centerIdToAvoid.toString());
    return this.http.get<any[]>(`${this.baseUrl}/product-with-all-stocks`, { headers });
  }

  getProductWithStockById(productId: number, centerId: number): Observable<any> {
    const headers = { centerId: centerId.toString(), productId: productId.toString() };
    return this.http.get<any>(`${this.baseUrl}/product-with-stock-for-id`, { headers });
  }
}

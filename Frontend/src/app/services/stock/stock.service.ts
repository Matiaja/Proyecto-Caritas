import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable, BehaviorSubject, tap, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class StockService {
  baseUrl = environment.baseUrl + 'stocks';
  private stockSubject = new BehaviorSubject<any[]>([]);
  stocks$ = this.stockSubject.asObservable();

  constructor(private http: HttpClient) {}

  getStocks() {
    this.http.get<any[]>(this.baseUrl).subscribe((stocks) => {
      this.stockSubject.next(stocks);
      console.log(stocks);
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
      }),
      catchError((error) => {
        console.error('Error al crear stock:', error);
        return throwError(() => new Error('No se pudo crear el stock'));
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

  validateQuantity(centerId: number, productId: number, newQuantity: number) {
    return this.http.get(`${this.baseUrl}/validate-quantity`, {
      params: { centerId, productId, newQuantity },
    });
  }

  getProductWithStock(centerId: number): Observable<any[]> {
    const headers = { centerId: centerId.toString() };
    return this.http.get<any[]>(`${this.baseUrl}/product-with-stock`, { headers });
  }

  getProductInStocks(productId: number): Observable<any[]> {
    const headers = new HttpHeaders().set('productId', productId.toString());
    return this.http.get<any[]>(`${this.baseUrl}/product-with-all-stocks`, { headers });
  }

  getProductWithStockById(productId: number, centerId: number): Observable<any> {
    const headers = { centerId: centerId.toString(), productId: productId.toString() };
    return this.http.get<any>(`${this.baseUrl}/product-with-stock-for-id`, { headers });
  }
}

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Product } from '../../models/product.model';
import { Observable, BehaviorSubject, tap, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class StockService {

  baseUrl = environment.baseUrl + 'stocks';
  private stockSubject = new BehaviorSubject<any[]>([]);
  stocks$ = this.stockSubject.asObservable();

  constructor(private http: HttpClient) { }

  getStocks(){
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
      })
    );
  }

  deleteStock(stockId: number){
    return this.http.delete(this.baseUrl + '/' + stockId).pipe(
      tap(() => {
        const currentStocks = this.stockSubject.getValue();
        const updatedStocks = currentStocks.filter((s) => s.id !== stockId);
        this.stockSubject.next(updatedStocks);
      })
    );
  }

  validateQuantity(productId: number, centerId: number, newQuantity: number): Observable<number> {
    return this.http.post<{ totalStock: number }>(`${this.baseUrl}/validate-quantity`, 
      { productId, centerId, newQuantity }
    ).pipe(
      map(response => response.totalStock),
      catchError(error => {
        console.error("Error validando stock:", error);
        return throwError(() => new Error(error.error.message || "Error en la validación del stock"));
      })
    );
  }

  getProductWithStock(centerId: number): Observable<any[]> {
    const headers = { 'centerId': centerId.toString() };
    return this.http.get<any[]>(`${this.baseUrl}/product-with-stock`, { headers });
  }

  getProductWithStockById(productId: number, centerId: number): Observable<any> {
    const headers = { 'centerId': centerId.toString(), 'productId': productId.toString() };
    return this.http.get<any>(`${this.baseUrl}/product-with-stock-for-id`, { headers });
  }

}

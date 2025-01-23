import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Product } from '../../models/product.model';
import { Observable, BehaviorSubject, tap } from 'rxjs';

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

  

}

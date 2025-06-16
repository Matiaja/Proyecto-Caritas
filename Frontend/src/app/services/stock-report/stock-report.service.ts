import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable, forkJoin, map } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ChartData {
  stockByCategory: CategoryChartData[];
  stockByType: TypeChartData[];
  stockByMonth: MonthChartData[];
  topProducts: ProductChartData[];
}

export interface CategoryChartData {
  categoryName: string;
  totalStock: number;
}

export interface TypeChartData {
  stockType: string;
  totalStock: number;
}

export interface MonthChartData {
  month: string;
  totalStock: number;
}

export interface ProductChartData {
  productName: string;
  totalStock: number;
}

@Injectable({
  providedIn: 'root'
})
export class StockReportService {
  baseUrl = environment.baseUrl;
  constructor(private http: HttpClient) { }

  getChartData(centerId: number): Observable<ChartData> {
    const headers = new HttpHeaders({ 'centerId': centerId.toString() });

    return forkJoin({
      categoryData: this.http.get<any[]>(`${this.baseUrl}Stocks/stock-by-category`, { headers }),
      topProducts: this.http.get<any[]>(`${this.baseUrl}Stocks/more-quantity-product-with-stock`, { headers }),
      allStocks: this.http.get<any[]>(`${this.baseUrl}Stocks/center-stocks`, { headers })
    }).pipe(
      map(({ categoryData, topProducts, allStocks }) => ({
        stockByCategory: categoryData.map(item => ({
          categoryName: item.categoryName,
          totalStock: item.totalStock
        })),
        stockByType: this.processStockByType(allStocks),
        stockByMonth: this.processStockByMonth(allStocks),
        topProducts: topProducts.map(item => ({
          productName: item.productName,
          totalStock: item.stockQuantity
        }))
      }))
    );
  }

  getProductsByCenter(centerId: number): Observable<any[]> {
    const headers = new HttpHeaders({ 'centerId': centerId.toString() });
    return this.http.get<any[]>(`${this.baseUrl}Stocks/product-with-stock`, { headers }).pipe(
      map((products: any[]) => {
        // Enriquecer los datos del producto con información de categoría
        return products.map(product => ({
          ...product,
          categoryId: product.categoryId || null
        }));
      })
    );
  }

  getCenters(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}Centers`);
  }

  private processStockByType(stocks: any[]): TypeChartData[] {
    const typeGroups = stocks.reduce((acc, stock) => {
      const type = stock.type || 'Sin tipo';
      acc[type] = (acc[type] || 0) + stock.quantity;
      return acc;
    }, {});

    return Object.entries(typeGroups).map(([type, total]) => ({
      stockType: type,
      totalStock: total as number
    }));
  }

  private processStockByMonth(stocks: any[]): MonthChartData[] {
    const monthGroups = stocks.reduce((acc, stock) => {
      const date = new Date(stock.date);
      const monthKey = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}`;
      
      if (!acc[monthKey]) {
        acc[monthKey] = 0;
      }
      
      acc[monthKey] += stock.type === 'Ingreso' ? stock.quantity : -stock.quantity;
      return acc;
    }, {});

    return Object.entries(monthGroups)
      .map(([month, total]) => ({
        month,
        totalStock: Math.max(0, total as number)
      }))
      .sort((a, b) => a.month.localeCompare(b.month))
      .slice(-12); // últimos 12 meses
  }
}

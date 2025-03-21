import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Product } from '../../models/product.model';
import { Observable, BehaviorSubject, tap, map, catchError, of} from 'rxjs';
import { PagedResult } from '../../models/paged-result.model';

@Injectable({
  providedIn: 'root'
})
export class ProductService {

  baseUrl = environment.baseUrl + 'products';
  private productsSubject = new BehaviorSubject<Product[]>([]);
  products$ = this.productsSubject.asObservable();
  totalItems: number = 0;
  constructor(private http: HttpClient) { }

  getProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(this.baseUrl).pipe(
      tap((products) => {
        this.productsSubject.next(products);
      })
    );
  }  

  getProductById(productId: number): Observable<Product> {
    return this.http.get<Product>(this.baseUrl + '/' + productId);
  }

  createProduct(product: any): Observable<Product> {
    return this.http.post<Product>(this.baseUrl, product).pipe(
      tap((newProduct) => {
        const currentProducts = this.productsSubject.getValue();
        this.productsSubject.next([...currentProducts, newProduct]);
      })
    );
  }

  deleteProduct(productId: number){
    return this.http.delete(this.baseUrl + '/' + productId).pipe(
      tap(() => {
        const currentProducts = this.productsSubject.getValue();
        const updatedProducts = currentProducts.filter((p) => p.id !== productId);
        this.productsSubject.next(updatedProducts);
      })
    );
  }

  searchProducts(searchTerm: string): Observable<Product[]> {
    console.log('Searching:', searchTerm);
    return this.http.get<Product[]>(`${this.baseUrl}/search?query=${searchTerm}`).pipe(
      catchError((error) => {
        console.error('Error searching products:', error);
        return of([]);
      })
    );
  }

  getFilteredProducts(categoryId?: number, sortBy?: string, order: string = 'asc'): void {
    let params = new HttpParams();
  
    if (categoryId) {
      params = params.set('categoryId', categoryId.toString());
    }
    if (sortBy) {
      params = params.set('sortBy', sortBy);
      params = params.set('order', order);
    }
  
    this.http.get<any[]>(`${this.baseUrl}/filter`, { params }).subscribe(products => {
      this.productsSubject.next(products); // Envía todos los elementos al componente
    });
  }

}

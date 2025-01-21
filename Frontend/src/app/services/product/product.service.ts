import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Product } from '../../models/product.model';
import { Observable, BehaviorSubject, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProductService {

  baseUrl = environment.baseUrl + 'products';
  private productsSubject = new BehaviorSubject<Product[]>([]);
  products$ = this.productsSubject.asObservable();

  constructor(private http: HttpClient) { }

  getProducts(){
    this.http.get<Product[]>(this.baseUrl).subscribe((products) => {
      this.productsSubject.next(products);
    });
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



}

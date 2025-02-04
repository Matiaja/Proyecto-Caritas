import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Product } from '../../models/product.model';
import { map, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProductService {

  baseUrl = environment.baseUrl;
  constructor(private http: HttpClient) { }

  getProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(this.baseUrl + 'products');
  }

  getProductById(productId: number): Observable<Product> {
    return this.http.get<Product>(this.baseUrl + 'products/' + productId);
  }

  searchProducts(name: string): Observable<Product[]> {
    return this.getProducts().pipe(
      map(products => 
        products.filter(prod => 
          prod.name.toLowerCase().includes(name.toLowerCase())
        )
      )
    );
  }

}

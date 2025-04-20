import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductService } from '../../../services/product/product.service';
import { UiTableComponent } from '../../../shared/components/ui-table/ui-table.component';
import { CommonModule, Location } from '@angular/common';
import { BreadcrumbComponent } from '../../../shared/components/breadcrumbs/breadcrumbs.component';
import { Product } from '../../../models/product.model';
import { CategoryService } from '../../../services/category/category.service';
import { switchMap } from 'rxjs';
@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [UiTableComponent, CommonModule, BreadcrumbComponent],
  templateUrl: './product-detail.component.html',
  styleUrl: './product-detail.component.css',
})
export class ProductDetailComponent implements OnInit {
  product: Product = {
    id: 0,
    name: '',
    categoryId: 0,
    stocks: [],
  };

  category: any = {
    name: '',
  };

  title = 'Detalle de producto';
  columnHeaders: Record<string, string> = {
    id: 'ID',
    name: 'Nombre',
    description: 'Descripción',
    stock: 'Stock',
  };

  constructor(
    private productService: ProductService,
    private categoryService: CategoryService,
    private route: ActivatedRoute,
    private router: Router,
    private location: Location
  ) {}

  ngOnInit(): void {
    this.route.params
      .pipe(
        switchMap((params) => {
          const productId = params['id'];
          return this.productService.getProductById(productId);
        }),
        switchMap((product: Product) => {
          this.product = product;
          return this.categoryService.getCategory(product.categoryId);
        })
      )
      .subscribe({
        next: (category: any) => {
          this.category = category;
        },
      });
  }

  loadProductDetails(productId: any): void {
    this.productService.getProductById(productId).subscribe({
      next: (product: Product) => {
        this.product = product;
      },
    });
  }

  loadCategory(categoryId: any): void {
    this.categoryService.getCategory(categoryId).subscribe({
      next: (category: any) => {
        this.category = category;
      },
    });
  }

  goBack() {
    this.location.back();
  }

  onAddElement = null;
  onEditElement = null;
  onDeleteElement = null;
}

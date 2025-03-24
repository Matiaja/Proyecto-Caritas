import { Component, OnInit } from '@angular/core';
import { UiTableComponent } from '../../shared/components/ui-table/ui-table.component';
import { MatDialog } from '@angular/material/dialog';
import { GenericFormModalComponent } from '../../shared/components/generic-form-modal/generic-form-modal.component';
import { Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { BreadcrumbComponent } from '../../shared/components/breadcrumbs/breadcrumbs.component';
import { ConfirmModalService } from '../../services/confirmModal/confirm-modal.service';
import { ProductService } from '../../services/product/product.service';
import { ToastrService } from 'ngx-toastr';
import { CategoryService } from '../../services/category/category.service';
import { NgxPaginationModule } from 'ngx-pagination';

@Component({
  selector: 'app-product',
  standalone: true,
  imports: [NgxPaginationModule, UiTableComponent, BreadcrumbComponent],
  templateUrl: './product.component.html',
  styleUrl: './product.component.css'
})

export class ProductComponent implements OnInit {
  paginationId = 'productPagination';
  title = 'Productos';
  displayedColumns = ['name', 'code', 'categoryName', 'quantity'];
  searchColumns = ['name', 'code'];
  products: any[] = [];
  selectedCategory: number | null = null;
  sortBy: string = '';
  order: string = 'asc';
  categories: any[] = [];
  columnHeaders: { [key: string]: string } = {
    name: 'Nombre',
    code: 'Code',
    categoryName: 'Categoría',
    quantity: 'Stock',
  };
  sortOptions = [
    { key: 'name', label: 'Nombre' },
    { key: 'quantity', label: 'Cantidad' }
  ];

  page = 1;
  itemsPerPage: number = 10;
  totalItems: number = 0;

  constructor(
    private productService: ProductService, 
    private router: Router, 
    private modalService: ConfirmModalService,
    private toastr: ToastrService,
    private categoryService: CategoryService
  ) { }
  ngOnInit() {
    this.loadCategories();

    // this.productService.products$.subscribe(products => {
    //   this.products = products;
    // });
    this.loadProducts();
  }

  loadCategories() {
    this.categoryService.categories$.subscribe(categories => {
      this.categories = categories;
    });
    this.categoryService.getCategories();
  }

  loadProducts() {
    this.productService.getFilteredProducts(
      this.selectedCategory ?? undefined,
      this.sortBy,
      this.order
    );

    this.productService.products$.subscribe(products => {
      this.products = products;
    });

    this.totalItems = this.productService.totalItems;
  }

  onFilterChange(filters: { categoryId?: number; sortBy?: string; order?: string }) {
    this.selectedCategory = filters.categoryId || null;
    this.sortBy = filters.sortBy || '';
    this.order = filters.order || 'asc';
    this.loadProducts();
  }

  // onCategoryChange(categoryId: string) {
  //   this.selectedCategory = categoryId ? Number(categoryId) : null;
  //   this.loadProducts();
  // }

  // onSortChange(sortField: string) {
  //   this.sortBy = sortField;
  //   this.loadProducts();
  // }

  // onOrderChange(order: string) {
  //   this.order = order;
  //   this.loadProducts();
  // }

  onPageChange(newPage: number) {
    this.page = newPage;
    this.loadProducts();
  }

  onAddProduct(): void {
    this.router.navigate(['/products/add']);
  }

  onEditProduct(product: any) {
  }

  onSelectProduct(product: any) {
    this.router.navigate(['/products/detail', product.id]);
  }

  async onDeleteProduct(product: any) {
    const confirmed = await this.modalService.confirm('Eliminar producto',
      '¿Estás seguro de que quieres eliminar este producto?'
    );

    if (confirmed) {
      this.productService.deleteProduct(product.id).subscribe(() => {
        this.products = this.products.filter((p) => p.id !== product.id);
      });
      this.toastr.success('Producto eliminado correctamente');
    }
  }
}

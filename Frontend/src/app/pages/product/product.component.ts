import { Component, OnInit } from '@angular/core';
import { UiTableComponent } from '../../shared/components/ui-table/ui-table.component';
import { MatDialog } from '@angular/material/dialog';
import { GenericFormModalComponent } from '../../shared/components/generic-form-modal/generic-form-modal.component';
import { Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { BreadcrumbComponent } from '../../shared/components/breadcrumbs/breadcrumbs.component';
import { ConfirmModalService } from '../../services/confirmModal/confirm-modal.service';
import { ProductService } from '../../services/product/product.service';

@Component({
  selector: 'app-product',
  standalone: true,
  imports: [UiTableComponent, BreadcrumbComponent],
  templateUrl: './product.component.html',
  styleUrl: './product.component.css'
})

export class ProductComponent implements OnInit {
  title = 'Productos';
  displayedColumns = ['name', 'description', 'stock'];
  products: any[] = [];
  columnHeaders: { [key: string]: string } = {
    name: 'Nombre',
    description: 'Descripción',
    stock: 'Stock',
  };

  constructor(private productService: ProductService, private router: Router, private modalService: ConfirmModalService) { }
  ngOnInit() {
    this.productService.products$.subscribe(products => {
      this.products = products;
    });
    this.productService.getProducts();

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
    }
  }
}

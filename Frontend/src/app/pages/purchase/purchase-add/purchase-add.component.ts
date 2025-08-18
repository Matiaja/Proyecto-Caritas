import { CommonModule } from '@angular/common';
import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PurchaseService } from '../../../services/purchase/purchase.service';
import { ProductService } from '../../../services/product/product.service';
import { GlobalStateService } from '../../../services/global/global-state.service';
import { MatFormField, MatFormFieldModule, MatLabel } from '@angular/material/form-field';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatInputModule } from '@angular/material/input';
import { MAT_DATE_LOCALE, provideNativeDateAdapter } from '@angular/material/core';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';

@Component({
  selector: 'app-purchase-add',
  standalone: true,
  providers: [{provide: MAT_DATE_LOCALE, useValue: 'es-AR'},
  provideNativeDateAdapter()
  ],
  imports: [
    FormsModule,
    ReactiveFormsModule,
    CommonModule,
    // Material Modules
    MatFormField,
    MatLabel,
    MatDatepickerModule,
    MatNativeDateModule,
    MatFormFieldModule,
    MatInputModule,
    FormsModule,
  ],
  templateUrl: './purchase-add.component.html',
  styleUrl: './purchase-add.component.css',
  encapsulation: ViewEncapsulation.None
})
export class PurchaseAddComponent implements OnInit {
  purchase: {
    purchaseDate: Date | null;
    type: string;
    centerId: number;
    items: { productId: number; productName: string; quantity: number, description?: string }[];
  } = {
    purchaseDate: null,
    type: '',
    centerId: 0,
    items: []
  };

  centers: any[] = [];
  products: any[] = [];
  filteredProducts: any[][] = [];
  selectedProduct: any = null;

  constructor(
    private purchaseService: PurchaseService,
    private productService: ProductService,
    private toastr: ToastrService,
    private router: Router,
    private globalStateService: GlobalStateService
  ) {}

  ngOnInit(): void {
    this.purchase.centerId = this.globalStateService.getCurrentCenterId();
    this.productService.getProducts().subscribe(data => this.products = data);
  }

  // Buscar producto
  filterProducts(index: number) {
    const searchTerm = this.purchase.items[index].productName?.toLowerCase() || '';
    if (!searchTerm.trim() || searchTerm.length < 3) {
      this.filteredProducts[index] = [];
      return;
    }
    this.filteredProducts[index] = this.products.filter(p =>
      p.name.toLowerCase().includes(searchTerm)
    );
  }

  selectProduct(index: number, product: any) {
    this.purchase.items[index].productId = product.id;
    this.purchase.items[index].productName = product.name;
    this.filteredProducts[index] = [];
  }

  addItem() {
    this.purchase.items.push({ productId: 0, productName: '', quantity: 1 });
    this.filteredProducts.push([]);
  }

  removeItem(index: number) {
    this.purchase.items.splice(index, 1);
    this.filteredProducts.splice(index, 1);
  }

  submit() {
      const purchase = {
        ...this.purchase,
        purchaseDate: this.purchase.purchaseDate ? this.purchase.purchaseDate.toISOString().split('T')[0] : undefined,
      };

      console.log('Purchase data to submit:', purchase);
    this.purchaseService.createPurchase(purchase).subscribe({
      next: (res: any) => {
        this.toastr.success('Compra registrada correctamente', 'Exito');
        this.router.navigate(['/purchases/' + res.id]);
      },
      error: (err) => {
        console.error(err);
        if(err.error && err.error.message) {
          this.toastr.error(err.error.message, 'Error');
        }
        else {
          this.toastr.error('Error al guardar la compra', 'Error');
        }
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/purchases']);
  }
}

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

@Component({
  selector: 'app-purchase-add',
  standalone: true,
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
  purchase: any = {
    purchaseDate: '',
    type: '',
    centerId: 0,
    items: []
  };

  centers: any[] = [];
  products: any[] = [];

  constructor(
    private purchaseService: PurchaseService,
    private productService: ProductService,
    private globalStateService: GlobalStateService
  ) {}

  ngOnInit(): void {
    this.purchase.centerId = this.globalStateService.getCurrentCenterId();
    this.productService.getProducts().subscribe(data => this.products = data);
  }

  addItem() {
    this.purchase.items.push({ productId: 0, quantity: 1 });
  }

  removeItem(index: number) {
    this.purchase.items.splice(index, 1);
  }

  submit() {
    this.purchaseService.createPurchase(this.purchase).subscribe({
      next: res => {
        alert('Compra registrada correctamente');
        this.purchase = { purchaseDate: '', type: '', centerId: this.globalStateService.getCurrentCenterId(), items: [] };
      },
      error: err => {
        console.error(err);
        alert('Error al guardar la compra');
      }
    });
  }
}

import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PurchaseService } from '../../../services/purchase/purchase.service';
import { ProductService } from '../../../services/product/product.service';
import { GlobalStateService } from '../../../services/global/global-state.service';
import { MAT_DATE_LOCALE, provideNativeDateAdapter } from '@angular/material/core';

@Component({
  selector: 'app-purchase-add',
  standalone: true,
  providers: [{provide: MAT_DATE_LOCALE, useValue: 'es-AR'},
  provideNativeDateAdapter()],
  imports: [
    FormsModule,
    ReactiveFormsModule,
    CommonModule,
  ],
  templateUrl: './purchase-add.component.html',
  styleUrl: './purchase-add.component.css'
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
    const purchaseToSend = {
      ...this.purchase,
      purchaseDate: this.purchase.purchaseDate
    };

    this.purchaseService.createPurchase(purchaseToSend).subscribe({
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

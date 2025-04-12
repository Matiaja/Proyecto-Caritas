import { Component, OnInit } from '@angular/core';
import { UiTableComponent } from '../../shared/components/ui-table/ui-table.component';
import { MatDialog } from '@angular/material/dialog';
import { GenericFormModalComponent } from '../../shared/components/generic-form-modal/generic-form-modal.component';
import { Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { BreadcrumbComponent } from '../../shared/components/breadcrumbs/breadcrumbs.component';
import { ConfirmModalService } from '../../services/confirmModal/confirm-modal.service';
import { StockService } from '../../services/stock/stock.service';
import { ProductService } from '../../services/product/product.service';
import { GlobalStateService } from '../../services/global/global-state.service';

@Component({
  selector: 'app-storage',
  standalone: true,
  imports: [UiTableComponent, BreadcrumbComponent],
  templateUrl: './storage.component.html',
  styleUrl: './storage.component.css'
})
export class StorageComponent implements OnInit{
  product = '';
  title = 'Almacén';
  displayedColumns = ['productName', 'productCode', 'stockQuantity'];
  stocks: any[] = [];
  columnHeaders: { [key: string]: string } = {
    productName: 'Nombre del producto',
    productCode: 'Código del producto',
    stockQuantity: 'Cantidad',
  };

  centerId: number | null = null;

  constructor(
    private stockService: StockService,
    private router: Router,
    private modalService: ConfirmModalService,
    private globalStateService: GlobalStateService
  ) {
    this.centerId = this.globalStateService.getCurrentCenterId();
  }

  ngOnInit() {
    if (this.centerId) {
      this.stockService.getProductWithStock(this.centerId).subscribe((stocks) => {
        this.stocks = stocks;
      });
    }
  }

  onAddStock(): void {
    this.router.navigate(['/storage/add']);
  }

  onSelectStock(stock: any) {
    this.router.navigate(['/storage/detail', stock.productId]);
  }

}

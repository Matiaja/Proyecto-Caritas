import { Component, OnInit } from '@angular/core';
import { UiTableComponent } from '../../shared/components/ui-table/ui-table.component';
import { Router } from '@angular/router';
import { BreadcrumbComponent } from '../../shared/components/breadcrumbs/breadcrumbs.component';
import { ConfirmModalService } from '../../services/confirmModal/confirm-modal.service';
import { StockService } from '../../services/stock/stock.service';
import { GlobalStateService } from '../../services/global/global-state.service';

@Component({
  selector: 'app-storage',
  standalone: true,
  imports: [UiTableComponent, BreadcrumbComponent],
  templateUrl: './storage.component.html',
  styleUrl: './storage.component.css',
})
export class StorageComponent implements OnInit {
  product = '';
  title = 'Almacén';
  displayedColumns = ['productName', 'productCode', 'stockQuantity'];
  stocks: any[] = [];
  mobileHeaders: { [key: string]: string } = {
    productName: 'Nombre del producto',
    stockQuantity: 'Cantidad',
  };
  mobileColumns = ['productName', 'stockQuantity'];
  searchColumns = ['productName', 'productCode'];

  columnHeaders: { [key: string]: string } = {
    productName: 'Nombre del producto',
    productCode: 'Código del producto',
    stockQuantity: 'Cantidad',
  };

  page = 1;
  itemsPerPage = 10;
  totalItems = 0;

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
    this.loadStock();
  }

  onPageChange(newPage: number) {
    this.page = newPage;
    this.loadStock();
  }

  loadStock() {
    if (this.centerId) {
      this.stockService.getProductWithStock(this.centerId).subscribe((stocks) => {
        console.log('Stocks recibidos:', stocks);
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

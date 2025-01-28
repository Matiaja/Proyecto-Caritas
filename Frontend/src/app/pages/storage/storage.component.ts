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
  displayedColumns = ['productName', 'productCode', 'stockQuantity', 'stockDate'];
  stocks: any[] = [];
  columnHeaders: { [key: string]: string } = {
    productName: 'Nombre del producto',
    productCode: 'Código del producto',
    stockQuantity: 'Cantidad',
    stockDate: 'Fecha',
  };

  constructor(private stockService: StockService, private router: Router, private modalService: ConfirmModalService) { }
  ngOnInit() {
    this.stockService.stocks$.subscribe((stocks) => {
      this.stocks = stocks.map((stock) => ({
        ...stock,
        productName: stock.product.name,
        productCode: stock.product.code,
        stockQuantity: stock.quantity,
        stockDate: stock.date,
      }));
    });
  
    this.stockService.getStocks();
  }

  onAddStock(): void {
    this.router.navigate(['/storage/add']);
  }

  onEditStock(stock: any) {
  }

  onSelectStock(stock: any) {
    this.router.navigate(['/storage/detail', stock.id]);
  }

  async onDeleteStock(stock: any) {
    const confirmed = await this.modalService.confirm('Eliminar stock',
      '¿Estás seguro de que quieres eliminar este stock?'
    );

    if (confirmed) {
      this.stockService.deleteStock(stock.id).subscribe(() => {
        this.stocks = this.stocks.filter((s) => s.id !== stock.id);
      });
    }
  }


}

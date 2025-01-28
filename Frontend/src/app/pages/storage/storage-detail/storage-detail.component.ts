import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Category } from '../../../models/category.model';
import { UiTableComponent } from "../../../shared/components/ui-table/ui-table.component";
import { CommonModule, Location } from '@angular/common';
import { BreadcrumbComponent } from '../../../shared/components/breadcrumbs/breadcrumbs.component';
import { StockService } from '../../../services/stock/stock.service';
import { ProductService } from '../../../services/product/product.service';

@Component({
  selector: 'app-storage-detail',
  standalone: true,
  imports: [UiTableComponent, CommonModule, BreadcrumbComponent],
  templateUrl: './storage-detail.component.html',
  styleUrl: './storage-detail.component.css'
})
export class StorageDetailComponent implements OnInit{
  stock: any = {
    id: 0,
    product: {
      name: '',
      code: '',
    },
    quantity: 0,
    date: '',
    expirationDate: '',
    description: '',
    weight: 0,
  };

  constructor(
    private stockService: StockService,
    private productService: ProductService,
    private route: ActivatedRoute,
    private router: Router,
    private location: Location
  ) { }

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      const stockId = params['id'];
      this.loadStockDetails(stockId);
    });
  }

  loadStockDetails(stockId: any): void {
    this.stockService.getStockById(stockId).subscribe({
      next: (stock) => {
        this.stock = stock;
      }
    });
  }

  goBack() {
    this.location.back();
  }

  onAddElement = null;
  onEditElement = null;
  onDeleteElement = null;
}

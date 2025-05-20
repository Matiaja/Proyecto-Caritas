import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { UiTableComponent } from '../../../shared/components/ui-table/ui-table.component';
import { CommonModule, Location } from '@angular/common';
import { BreadcrumbComponent } from '../../../shared/components/breadcrumbs/breadcrumbs.component';
import { StockService } from '../../../services/stock/stock.service';
import { ProductService } from '../../../services/product/product.service';
import { GlobalStateService } from '../../../services/global/global-state.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-storage-detail',
  standalone: true,
  imports: [UiTableComponent, CommonModule, BreadcrumbComponent, FormsModule],
  templateUrl: './storage-detail.component.html',
  styleUrl: './storage-detail.component.css',
})
export class StorageDetailComponent implements OnInit {
  stock: any[] = [
    {
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
    },
  ];

  centerId: number | null = null;

    filters = {
      type: '',
      fromDate: '',
      toDate: '',
    };

  constructor(
    private stockService: StockService,
    private productService: ProductService,
    private route: ActivatedRoute,
    private router: Router,
    private location: Location,
    private globalStateService: GlobalStateService
  ) {
    this.centerId = this.globalStateService.getCurrentCenterId();
  }

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      const productId = params['id'];
      this.loadProductStockDetails(productId);
    });
  }

  loadProductStockDetails(productId: any): void {
    if (this.centerId === null) {
      console.error('Center ID is null');
      return;
    }
    this.stockService.getProductWithStockById(productId, this.centerId).subscribe(
      (response) => {
        console.log(response);
        this.stock = response;
      },
      (error) => {
        console.error('Error al cargar los detalles del stock', error);
      }
    );
  }

  get dateRangeInvalid(): boolean {
    const from = this.filters.fromDate ? new Date(this.filters.fromDate) : null;
    const to = this.filters.toDate ? new Date(this.filters.toDate) : null;
    return from && to ? from > to : false;
  }

  get filteredStock() {
    if (this.dateRangeInvalid) return [];

    return this.stock.filter((item) => {
      const matchesType = this.filters.type ? item.type === this.filters.type : true;
      const itemDate = new Date(item.date);
      const from = this.filters.fromDate ? new Date(this.filters.fromDate) : null;
      const to = this.filters.toDate ? new Date(this.filters.toDate) : null;
      const matchesFrom = from ? itemDate >= from : true;
      const matchesTo = to ? itemDate <= to : true;
      return matchesType && matchesFrom && matchesTo;
    });
  }


  formatDate(dateStr: string): string {
    const date = new Date(dateStr);
    return date.toLocaleDateString('es-AR'); // Ej: 28/01/2025
  }

  goBack() {
    this.location.back();
  }

  onAddElement = null;
  onEditElement = null;
  onDeleteElement = null;
}

import { Component, OnInit } from '@angular/core';
import { UiTableComponent } from '../../shared/components/ui-table/ui-table.component';
import { Router } from '@angular/router';
import { BreadcrumbComponent } from '../../shared/components/breadcrumbs/breadcrumbs.component';
import { ConfirmModalService } from '../../services/confirmModal/confirm-modal.service';
import { StockService } from '../../services/stock/stock.service';
import { GlobalStateService } from '../../services/global/global-state.service';
import { CategoryService } from '../../services/category/category.service';
import { CenterService } from '../../services/center/center.service';
import { AuthService } from '../../auth/auth.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-storage',
  standalone: true,
  imports: [UiTableComponent, BreadcrumbComponent, FormsModule, CommonModule],
  templateUrl: './storage.component.html',
  styleUrl: './storage.component.css',
})
export class StorageComponent implements OnInit {
  product = '';
  isAdmin = false;
  centers: any[] = [];
  selectedCenter: number | null = null;
  groupByCenter = true;

  title = 'Almacén';
  showSelectButton = true;
  displayedColumns = ['productName', 'productCode', 'stockQuantity'];
  stocks: any[] = [];
  mobileHeaders: { [key: string]: string } = {
    productName: 'Producto',
    stockQuantity: 'Cantidad',
  };
  mobileColumns = ['productName', 'stockQuantity'];
  searchColumns = ['productName', 'productCode'];

  columnHeaders: { [key: string]: string } = {
    productName: 'Producto',
    productCode: 'Código',
    stockQuantity: 'Cantidad',
  };
  sortOptions = [
    { key: 'productName', label: 'Nombre' },
    { key: 'stockQuantity', label: 'Cantidad' },
  ];
  selectedCategory: number | null = null;
  sortBy = '';
  order = 'asc';
  categories: any[] = [];

  page = 1;
  itemsPerPage = 10;
  totalItems = 0;

  centerId!: number;

  constructor(
    private stockService: StockService,
    private router: Router,
    private modalService: ConfirmModalService,
    private globalStateService: GlobalStateService,
    private categoryService: CategoryService,
    private centerService: CenterService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.stockService.stocks$.subscribe((stocks) => {
      // Si groupByCenter está activado, agregamos el nombre del centro
      if (this.groupByCenter && this.selectedCenter == null) {
        this.stocks = stocks.map(stock => ({
          ...stock,
          centerName: this.getCenterNameById(stock.centerId)
        }));
      } else {
        this.stocks = stocks;
      }
    });

    this.isAdmin = this.authService.getUserRole() === 'Admin';
    
    this.centerId = this.globalStateService.getCurrentCenterId();
    
    if (this.isAdmin) {
      this.centerService.getCenters().subscribe((centers) => {
        this.centers = centers;
      });

    }
    this.selectedCenter = this.centerId;
    
    this.loadCategories();
    this.loadStock();
  }

  getCenterNameById(centerId: number): string {
    const center = this.centers.find(c => c.id === centerId);
    return center ? center.name : 'Desconocido';
  }

  onPageChange(newPage: number) {
    this.page = newPage;
    this.loadStock();
  }

  loadStock() {
    if (this.centerId) {
      if(!this.groupByCenter) {
        this.showSelectButton = false;
      } else {
        this.showSelectButton = true;
      }
      this.stockService.getProductWithStock(
        this.selectedCenter ?? null,
        this.selectedCategory ?? undefined,
        this.sortBy,
        this.order,
        this.groupByCenter
      );
      this.totalItems = this.stockService.totalItems;

      // Ajustar columnas visibles según si se agrupa por centro
      if (this.groupByCenter && this.selectedCenter == null) {
        this.displayedColumns = ['productName', 'productCode', 'centerName', 'stockQuantity'];
        this.mobileColumns = ['productName', 'centerName', 'stockQuantity'];
        this.columnHeaders['centerName'] = 'Centro';
        this.mobileHeaders['centerName'] = 'Centro';
      } else {
        this.displayedColumns = ['productName', 'productCode', 'stockQuantity'];
        this.mobileColumns = ['productName', 'stockQuantity'];

        // Si ya estaba agregado, lo sacamos
        delete this.columnHeaders['centerName'];
        delete this.mobileHeaders['centerName'];
      }
    }
  }

  loadCategories() {
    this.categoryService.categories$.subscribe((categories) => {
      this.categories = categories;
    });
    this.categoryService.getCategories();
  }

  filterStocks() {
    this.groupByCenter = true; // si selecciona todos, al inicio se muestra desglozado
    this.loadStock();
  }

  onFilterChange(filters: { categoryId?: number; sortBy?: string; order?: string }) {
    this.selectedCategory = filters.categoryId || null;
    this.sortBy = filters.sortBy || '';
    this.order = filters.order || 'asc';
    this.loadStock();
  }

  onAddStock(): void {
    this.router.navigate(['/storage/add']);
  }

  onSelectStock(stock: any) {
    console.log(stock);
    const extras = this.isAdmin ? { queryParams: { centerId: stock.centerId } } : {};
    this.router.navigate(['/storage/detail', stock.productId], extras);
  }
}

import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { UiTableComponent } from '../../../shared/components/ui-table/ui-table.component';
import { CommonModule, Location } from '@angular/common';
import { BreadcrumbComponent } from '../../../shared/components/breadcrumbs/breadcrumbs.component';
import { StockService } from '../../../services/stock/stock.service';
import { ProductService } from '../../../services/product/product.service';
import { GlobalStateService } from '../../../services/global/global-state.service';
import { FormsModule } from '@angular/forms';
import { PdfService } from '../../../services/pdf/pdf.service';

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
    private globalStateService: GlobalStateService,
    private pdfService: PdfService
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      const productId = params['id'];
      this.route.queryParams.subscribe((queryParams) => {
        const queryCenterId = queryParams['centerId'];
        this.centerId = queryCenterId ? +queryCenterId : this.globalStateService.getCurrentCenterId();
        
        this.loadProductStockDetails(productId);
      });
    });
  }

  loadProductStockDetails(productId: any): void {
    if (this.centerId === null) {
      console.error('Center ID is null');
      return;
    }
    this.stockService.getProductWithStockById(productId, this.centerId).subscribe(
      (response) => {
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


  generatePdf(): void {
    if (!this.stock || this.stock.length === 0) {
      console.error('No hay datos de stock para generar PDF');
      return;
    }

    const productData = {
      name: this.stock[0].product.name,
      code: this.stock[0].product.code
    };

    const centerData = {
      name: 'Traer nombre del centro'
    };

    this.pdfService.generateStockDetailPdfWithData(productData, centerData, this.filteredStock).subscribe({
      next: (blob) => {
        const filename = `detalle_stock_${this.stock[0].product.name}_${new Date().toISOString().split('T')[0]}.pdf`;
        this.pdfService.downloadPdf(blob, filename);
      },
      error: (error) => {
        console.error('Error generating PDF:', error);
      }
    });
  }

  // Custom metodos para generar PDF con más detalles
  generateCustomPdf(): void {
    if (!this.stock || this.stock.length === 0) return;

    const pdfRequest = {
      title: `Detalle de Stock - ${this.stock[0].product.name}`,
      subtitle: `Código: ${this.stock[0].product.code}`,
      sections: [
        {
          title: 'Información del Producto',
          keyValuePairs: [
            { key: 'Nombre', value: this.stock[0].product.name },
            { key: 'Código', value: this.stock[0].product.code },
            { key: 'ID del Producto', value: this.stock[0].productId.toString() }
          ]
        }
      ],
      tableData: [{
        title: 'Movimientos de Stock',
        headers: ['Tipo', 'Cantidad', 'Descripción', 'Origen', 'Fecha', 'Fecha Exp.', 'Peso'],
        rows: this.filteredStock.map(item => [
          item.type || '-',
          item.quantity?.toString() || '-',
          item.description || '-',
          item.origin || '-',
          new Date(item.date).toLocaleDateString('es-AR'),
          item.expirationDate ? new Date(item.expirationDate).toLocaleDateString('es-AR') : '-',
          item.weight && item.weight > 0 ? item.weight.toString() : '-'
        ])
      }],
      footer: `Generado el ${new Date().toLocaleDateString('es-AR')} por Sistema Cáritas`
    };

    this.pdfService.generatePdf(pdfRequest).subscribe({
      next: (blob) => {
        const filename = `detalle_stock_${this.stock[0].product.name}_${new Date().toISOString().split('T')[0]}.pdf`;
        this.pdfService.downloadPdf(blob, filename);
      },
      error: (error) => {
        console.error('Error generating custom PDF:', error);
      }
    });
  }

  onAddElement = null;
  onEditElement = null;
  onDeleteElement = null;
}

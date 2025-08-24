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
import { CenterService } from '../../../services/center/center.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-storage-detail',
  standalone: true,
  imports: [UiTableComponent, CommonModule, BreadcrumbComponent, FormsModule],
  templateUrl: './storage-detail.component.html',
  styleUrl: './storage-detail.component.css',
})
export class StorageDetailComponent implements OnInit {
  today = new Date().toISOString().split('T')[0];
  product: any = null; // Para almacenar los datos del producto
  stock: any[] = []; // Inicializar como arreglo vacío

  centerId: number | null = null;
  centerName: string = '';

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
    private pdfService: PdfService,
    private centerService: CenterService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      const productId = params['id'];
      this.route.queryParams.subscribe((queryParams) => {
        const queryCenterId = queryParams['centerId'];
        this.centerId = queryCenterId ? +queryCenterId : this.globalStateService.getCurrentCenterId();
        if (this.centerId !== null) {
          this.fetchCenterName(this.centerId);
        }
        this.loadProduct(productId);
        this.loadProductStockDetails(productId);
      });
    });
  }

  loadProduct(productId: number): void {
    this.productService.getProductById(productId).subscribe({
      next: (product) => {
        this.product = product; // Asignar datos del producto
      },
      error: (error) => {
        this.toastr.error('Producto no encontrado', 'Error');
        console.error('Error al cargar el producto:', error);
        this.goBack();
      },
    });
  }

  loadProductStockDetails(productId: any): void {
    if (this.centerId === null) {
      this.toastr.error('ID del centro no disponible', 'Error');
      console.error('Center ID is null');
      this.goBack();
      return;
    }
    this.stockService.getProductWithStockById(productId, this.centerId).subscribe({
      next: (response) => {
        this.stock = response || []; // Asegurar que stock sea un arreglo
        this.applyFilters(); // Aplicar filtros iniciales
      },
      error: (error) => {
        this.toastr.error('Error al cargar los movimientos de stock', 'Error');
        console.error('Error al cargar los detalles del stock:', error);
        this.stock = []; // Asegurar que stock sea un arreglo vacío en caso de error
      },
    });
  }

  applyFilters(): void {
    // Forzar la re-evaluación de filteredStock
    this.filters = { ...this.filters }; // Crear una nueva referencia para activar cambio de detección
  }

  private fetchCenterName(centerId: number) {
    this.centerService.getCenters().subscribe({
      next: (centers: any[]) => {
        const c = centers.find(c => c.id === centerId);
        this.centerName = c ? c.name : `Centro #${centerId}`;
      },
      error: () => {
        this.centerName = `Centro #${centerId}`;
      }
    });
  }

  get dateRangeInvalid(): boolean {
    const from = this.filters.fromDate ? new Date(this.filters.fromDate) : null;
    const to = this.filters.toDate ? new Date(this.filters.toDate) : null;
    return from && to ? from > to : false;
  }

  get filteredStock(): any[] {
  if (this.dateRangeInvalid) return [];

  const fs = this.stock.filter((item) => {
    const matchesType = this.filters.type ? item.type === this.filters.type : true;

    // Convertir item.date de dd/MM/yyyy a yyyy-MM-dd
    const dateParts = item.date.split('/');
    const itemDate = dateParts.length === 3 ? new Date(`${dateParts[2]}-${dateParts[1]}-${dateParts[0]}`) : null;

    const from = this.filters.fromDate ? new Date(this.filters.fromDate) : null;
    if (from) from.setHours(0, 0, 0, 0);
    const to = this.filters.toDate ? new Date(this.filters.toDate) : null;
    if (to) to.setHours(23, 59, 59, 999);

    console.log('Item Date:', itemDate, 'From:', from, 'To:', to); // Depuración

    // Solo aplicar filtrado por fecha si itemDate es válido
    const matchesFrom = !itemDate || !from ? true : itemDate >= from;
    const matchesTo = !itemDate || !to ? true : itemDate <= to;

    return matchesType && matchesFrom && matchesTo;
  });

  console.log('Filtered Stock:', fs); // Depuración
  return fs;
}

  formatDate(dateStr: string): string {
    const date = new Date(dateStr);
    return date.toLocaleDateString('es-AR'); // Ej: 28/01/2025
  }

  goBack(): void {
    this.location.back();
  }

  generatePdf(): void {
    if (!this.product || !this.filteredStock.length) {
      this.toastr.error('No hay datos para generar el PDF', 'Error');
      return;
    }
    const productData = {
      name: this.product.name,
      code: this.product.code,
    };
    const centerData = {
      name: this.centerName || 'Centro no disponible'
    };
    this.pdfService.generateStockDetailPdfWithData(productData, centerData, this.filteredStock).subscribe({
      next: (blob) => {
        const filename = `detalle_stock_${this.product.name}_${new Date().toISOString().split('T')[0]}.pdf`;
        this.pdfService.downloadPdf(blob, filename);
        this.toastr.success('PDF generado con éxito', 'Éxito');
      },
      error: (error) => {
        this.toastr.error('Error al generar el PDF', 'Error');
        console.error('Error generating PDF:', error);
      },
    });
  }

  generateCustomPdf(): void {
    if (!this.product || !this.filteredStock.length) {
      this.toastr.error('No hay datos para generar el PDF', 'Error');
      return;
    }

    const pdfRequest = {
      title: `Detalle de Stock - ${this.product.name}`,
      subtitle: `Código: ${this.product.code}`,
      sections: [
        {
          title: 'Información del Producto',
          keyValuePairs: [
            { key: 'Nombre', value: this.product.name },
            { key: 'Código', value: this.product.code },
            { key: 'ID del Producto', value: this.product.id.toString() },
          ],
        },
      ],
      tableData: [
        {
          title: 'Movimientos de Stock',
          headers: ['Tipo', 'Cantidad', 'Descripción', 'Origen', 'Fecha', 'Fecha Exp.', 'Peso'],
          rows: this.filteredStock.map((item) => [
            item.type || '-',
            item.quantity?.toString() || '-',
            item.description || '-',
            item.origin || '-',
            this.formatDate(item.date),
            item.expirationDate ? this.formatDate(item.expirationDate) : '-',
            item.weight && item.weight > 0 ? item.weight.toString() : '-',
          ]),
        },
      ],
      footer: `Generado el ${new Date().toLocaleDateString('es-AR')} por Sistema Cáritas`,
    };

    this.pdfService.generatePdf(pdfRequest).subscribe({
      next: (blob) => {
        const filename = `detalle_stock_${this.product.name}_${new Date().toISOString().split('T')[0]}.pdf`;
        this.pdfService.downloadPdf(blob, filename);
        this.toastr.success('PDF personalizado generado con éxito', 'Éxito');
      },
      error: (error) => {
        this.toastr.error('Error al generar el PDF personalizado', 'Error');
        console.error('Error generating custom PDF:', error);
      },
    });
  }

  onAddElement = null;
  onEditElement = null;
  onDeleteElement = null;
}
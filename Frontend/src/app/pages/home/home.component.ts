import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StockReportService, ProductStockSummary, StockHistory } from '../../services/stock-report/stock-report.service';
import { CategoryService } from '../../services/category/category.service';
import { ProductService } from '../../services/product/product.service';
import { CenterService } from '../../services/center/center.service';
import { AuthService } from '../../auth/auth.service';
import { ChartData, ChartOptions, Chart, Plugin } from 'chart.js';
import DataLabelsPlugin from 'chartjs-plugin-datalabels';
import { PdfService } from '../../services/pdf/pdf.service';
import { NgChartsModule } from 'ng2-charts';
import { forkJoin } from 'rxjs';
import { GlobalStateService } from '../../services/global/global-state.service'; // OPCIONAL si existe

interface Category {
  id: number;
  name: string;
}

interface Product {
  id: number;
  name: string;
}

interface Center {
  id: number;
  name: string;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule, NgChartsModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
})
export class HomeComponent implements OnInit {
  stockData: ProductStockSummary[] = [];
  categories: Category[] = [];
  products: Product[] = [];
  centers: Center[] = [];
  isLoading = false;
  isAdmin = false;

  // Filtros
  centerId?: number;
  categoryId?: number;
  productId?: number;
  fromDate?: string;
  toDate?: string;

  // Selector de tipo: 'stock' o 'movimientos'
  viewType: 'stock' | 'movimientos' = 'stock';

  // Chart Data
  pieChartData: ChartData<'pie'> = { labels: [], datasets: [] };
  barChartData: ChartData<'bar'> = { labels: [], datasets: [] };
  lineChartData: ChartData<'line'> = { labels: [], datasets: [] };
  doughnutChartData: ChartData<'doughnut'> = { labels: [], datasets: [] };

  // Chart Options
  pieChartOptions: ChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'bottom',
        labels: {
          boxWidth: 12,
          font: { size: 11 }
        }
      },
      // @ts-ignore
      datalabels: {
        color: '#fff',
        font: { size: 11, weight: 'bold' },
        formatter: (value: any, ctx: any) => {
          const total = (ctx.chart.data.datasets[0].data as number[]).reduce((s: number, n: number) => s + n, 0) || 1;
          return value + ' (' + ((value / total) * 100).toFixed(1) + '%)';
        }
      }
    }
  };

  barChartOptions: ChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      // @ts-ignore - plugin key added dynamically
      datalabels: {
        anchor: 'end',
        align: 'end',
        color: '#36337f',
        font: { size: 10, weight: 'bold' },
        formatter: (value: any) => value
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: { font: { size: 10 } }
      },
      x: {
        ticks: { 
          maxRotation: 45,
          font: { size: 10 }
        }
      }
    }
  };

  lineChartOptions: ChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'top',
        labels: { font: { size: 11 } }
      },
      // @ts-ignore
      datalabels: {
        align: 'top',
        anchor: 'end',
        color: '#36337f',
        font: { size: 9 },
        formatter: (value: any) => value
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: { font: { size: 10 } }
      },
      x: {
        ticks: { font: { size: 10 } }
      }
    }
  };

  doughnutChartOptions: ChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'bottom',
        labels: {
          boxWidth: 12,
          font: { size: 11 }
        }
      },
      // @ts-ignore
      datalabels: {
        color: '#fff',
        font: { size: 11, weight: 'bold' },
        formatter: (value: any, ctx: any) => {
          const total = (ctx.chart.data.datasets[0].data as number[]).reduce((s: number, n: number) => s + n, 0) || 1;
          return value + ' (' + ((value / total) * 100).toFixed(1) + '%)';
        }
      }
    }
  };

  constructor(
    private stockReportService: StockReportService,
    private categoryService: CategoryService,
    private productService: ProductService,
    private centerService: CenterService,
    private authService: AuthService,
    private pdfService: PdfService,
    private globalStateService?: GlobalStateService // opcional
  ) {}

  ngOnInit(): void {
    if (!(Chart as any).registeredDataLabels) {
      Chart.register(DataLabelsPlugin as unknown as Plugin); (Chart as any).registeredDataLabels = true;
    }
    this.checkAdminRole();
    this.loadInitialData();
  }

  checkAdminRole(): void {
    this.isAdmin = this.authService.isAdmin();
  }

  loadInitialData(): void {
    this.isLoading = true;
    
    const requests = [
      this.categoryService.getAllCategories(),
      this.productService.getProducts()
    ];

    if (this.isAdmin) {
      requests.push(this.centerService.getCenters());
    }

    forkJoin(requests).subscribe({
      next: (responses) => {
        this.categories = responses[0].map((cat: any) => ({ id: cat.id, name: cat.name }));
        this.products = responses[1].map((prod: any) => ({ id: prod.id, name: prod.name }));
        if (this.isAdmin && responses[2]) {
          this.centers = responses[2].map((center: any) => ({ id: center.id, name: center.name }));
        } else if (this.isAdmin) {
          this.centers = [];
          console.warn('Centers could not be loaded, but continuing with dashboard');
        }
        this.loadData();
      },
      error: (error) => {
        console.error('Error loading initial data:', error);
        
        // Try to load categories and products individually
        this.categoryService.getAllCategories().subscribe({
          next: (categories) => {
            this.categories = categories.map((cat: any) => ({ id: cat.id, name: cat.name }));
          },
          error: (err) => console.error('Error loading categories:', err)
        });

        this.productService.getProducts().subscribe({
          next: (products) => {
            this.products = products.map((prod: any) => ({ id: prod.id, name: prod.name }));
          },
          error: (err) => console.error('Error loading products:', err)
        });

        if (this.isAdmin) {
          // Try to load centers separately
          this.centerService.getCenters().subscribe({
            next: (centers) => {
              this.centers = centers.map((center: any) => ({ id: center.id, name: center.name }));
            },
            error: (err) => {
              console.error('Error loading centers:', err);
              this.centers = [];
            }
          });
        }

        // Continue with loading stock data even if some services failed
        this.loadData();
      }
    });
  }

  loadData(): void {
    this.isLoading = true;

    const cleanCenterId = this.centerId === undefined ? undefined : this.centerId;
    const cleanCategoryId = this.categoryId === undefined ? undefined : this.categoryId;
    const cleanProductId = this.productId === undefined ? undefined : this.productId;
    const cleanFromDate = !this.fromDate ? undefined : this.fromDate;
    const cleanToDate = !this.toDate ? undefined : this.toDate;

    const obs = this.viewType === 'stock'
      ? this.stockReportService.getStockReport(
          cleanCenterId,
          cleanCategoryId,
            cleanProductId,
          cleanFromDate,
          cleanToDate
        )
      : this.stockReportService.getPurchaseDistributionReport(
          cleanCenterId,
          cleanCategoryId,
          cleanProductId,
          cleanFromDate,
          cleanToDate
        );

    obs.subscribe({
      next: (data) => {
        // Normalize potential missing fields (defensive)
        this.stockData = (data || []).map(item => ({
          ...item,
          totalIngresos: item.totalIngresos ?? 0,
          totalEgresos: item.totalEgresos ?? 0,
          totalStock: item.totalStock ?? (item.totalIngresos ?? 0) - (item.totalEgresos ?? 0),
          lastMovementDate: item.lastMovementDate ?? new Date().toISOString(),
          movementCount: item.movementCount ?? ((item.totalIngresos ?? 0) + (item.totalEgresos ?? 0))
        }));
        this.generateCharts();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading data:', error);
        this.stockData = [];
        this.generateCharts();
        this.isLoading = false;
      }
    });
  }

  onViewTypeChange(): void {
    this.loadData();
  }

  onCenterChange(): void {
    // Explicitly handle center filter change
    if (this.centerId === undefined || this.centerId === null) {
      this.centerId = undefined;
    }
    this.loadData();
  }

  onCategoryChange(): void {
    // Explicitly handle category filter change
    if (this.categoryId === undefined || this.categoryId === null) {
      this.categoryId = undefined;
    }
    this.loadData();
  }

  onProductChange(): void {
    // Explicitly handle product filter change
    if (this.productId === undefined || this.productId === null) {
      this.productId = undefined;
    }
    this.loadData();
  }

  onDateChange(): void {
    // Handle date changes
    if (this.fromDate === '') {
      this.fromDate = undefined;
    }
    if (this.toDate === '') {
      this.toDate = undefined;
    }
    this.loadData();
  }

  extractFiltersData(): void {
    // Extract unique categories
    const categoryMap = new Map<number, string>();
    const productMap = new Map<number, string>();
    
    this.stockData.forEach(item => {
      categoryMap.set(item.categoryId, item.categoryName);
      productMap.set(item.productId, item.productName);
    });

    this.categories = Array.from(categoryMap.entries()).map(([id, name]) => ({ id, name }));
    this.products = Array.from(productMap.entries()).map(([id, name]) => ({ id, name }));
  }

  generateCharts(): void {
    this.generateCategoryChart();
    this.generateTopProductsChart();
    if (this.shouldShowTimeChart()) {
      this.generateStockOverTimeChart();
    } else {
      this.lineChartData = { labels: [], datasets: [] };
    }
    this.generateIngresoEgresoChart();
  }

  generateCategoryChart(): void {
    const grouped = this.groupSum(this.stockData, item => item.categoryName);
    const labels = Object.keys(grouped);
    const data = Object.values(grouped);
    
    this.pieChartData = {
      labels,
      datasets: [{
        data,
        backgroundColor: [
          '#36337f', '#5a67d8', '#63b3ed', '#68d391', '#f6e05e',
          '#fc8181', '#d69e2e', '#9f7aea', '#4fd1c7', '#f093fb'
        ],
        borderWidth: 2,
        borderColor: '#ffffff'
      }]
    };
  }

  private getCenterNameForPdf(): string {
    if (this.isAdmin) {
      if (this.centerId === undefined) return 'Todos';
      const c = this.centers.find(c => c.id === this.centerId);
      return c ? c.name : `Centro #${this.centerId}`;
    }
    // Usuario no admin: intentar obtener del estado global si se dispone
    if (this.centerId !== undefined) {
      const c = this.centers.find(c => c.id === this.centerId);
      if (c) return c.name;
    }
    const gsCenterName = (this.globalStateService as any)?.getCurrentCenterName?.();
    return gsCenterName || 'Asignado';
  }

  private getCategoryName(): string {
    if (this.categoryId === undefined) return 'Todas';
    const cat = this.categories.find(c => c.id === this.categoryId);
    return cat ? cat.name : `Cat #${this.categoryId}`;
  }

  private getProductName(): string {
    if (this.productId === undefined) return 'Todos';
    const prod = this.products.find(p => p.id === this.productId);
    return prod ? prod.name : `Prod #${this.productId}`;
  }

  private buildFiltersSummaryKeyValue(): { key: string; value: string }[] {
    const result: { key: string; value: string }[] = [];

    // Tipo
    result.push({
      key: 'Tipo',
      value: this.viewType === 'stock' ? 'Stock' : 'Compras y Distribuciones'
    });

    // Centro
    const center = this.getCenterNameForPdf();
    result.push({
      key: 'Centro',
      value: center ? center : 'todos los centros'
    });

    // Categoría
    const category = this.getCategoryName();
    result.push({
      key: 'Categoría',
      value: category && category !== 'Todas' ? category : 'todas las categorías'
    });

    // Producto
    const product = this.getProductName();
    result.push({
      key: 'Producto',
      value: product && product !== 'Todos' ? product : 'todos los productos'
    });

    // Fechas
    const from = this.fromDate ? new Date(this.fromDate).toLocaleDateString('es-AR') : null;
    const to = this.toDate ? new Date(this.toDate).toLocaleDateString('es-AR') : null;

    let fechaDesc = '';
    if (!from && !to) {
      fechaDesc = 'histórico';
    } else if (from && !to) {
      fechaDesc = `a partir del ${from}`;
    } else if (!from && to) {
      fechaDesc = `hasta el ${to}`;
    } else if (from && to) {
      fechaDesc = `entre el ${from} y el ${to}`;
    }

    result.push({
      key: 'Período',
      value: fechaDesc
    });

    return result;
  }

  exportChartsToPdf(): void {
    const charts: { title?: string; base64: string }[] = [];
    const capture = (selector: string, title: string) => {
      const el = document.querySelector(selector) as HTMLCanvasElement | null;
      if (el) charts.push({ title, base64: el.toDataURL('image/png') });
    };
    capture('.chart-container:nth-of-type(1) canvas', 'Stock por Categoría');
    capture('.chart-container:nth-of-type(2) canvas', 'Top 10 Productos');
    if (this.shouldShowTimeChart()) capture('.chart-container:nth-of-type(3) canvas', 'Evolución en el Tiempo');
    capture('.chart-container:nth-of-type(4) canvas', 'Ingresos vs Egresos');

    const filtersKeyValue = this.buildFiltersSummaryKeyValue();

    const req: any = {
      title: this.viewType === 'stock' ? 'Reporte de Stock' : 'Reporte de Movimientos',
      sections: [
        {
          title: 'Filtros Aplicados',
            keyValuePairs: filtersKeyValue
        }
      ],
      chartImages: charts.map(c => ({ title: c.title, base64: c.base64, widthPercent: 100 })),
      footer: 'Generado automáticamente por Sistema Cáritas'
    };

    this.pdfService.generatePdf(req).subscribe({
      next: (blob: Blob) => this.pdfService.openPdfInNewTab(blob),
      error: (err: any) => console.error('Error generating charts PDF', err)
    });
  }

  generateTopProductsChart(): void {
    const grouped = this.groupSum(this.stockData, item => item.productName);
    const top = Object.entries(grouped)
      .filter(([_, value]) => value > 0)
      .sort((a, b) => b[1] - a[1])
      .slice(0, 10);
    
    this.barChartData = {
      labels: top.map(x => x[0]),
      datasets: [{
        data: top.map(x => x[1]),
        label: 'Stock Total',
        backgroundColor: '#36337f',
        borderColor: '#36337f',
        borderWidth: 1
      }]
    };
  }

  generateStockOverTimeChart(): void {
    if (!this.shouldShowTimeChart()) {
      this.lineChartData = { labels: [], datasets: [] };
      return;
    }

    const cleanCenterId = this.centerId === undefined ? undefined : this.centerId;
    const cleanCategoryId = this.categoryId === undefined ? undefined : this.categoryId;
    const cleanProductId = this.productId === undefined ? undefined : this.productId;
    const cleanFromDate = !this.fromDate ? undefined : this.fromDate;
    const cleanToDate = !this.toDate ? undefined : this.toDate;

    const history$ = this.viewType === 'stock'
      ? this.stockReportService.getStockHistory(
          cleanCenterId,
          cleanCategoryId,
          cleanProductId,
          cleanFromDate,
          cleanToDate
        )
      : this.stockReportService.getPurchaseDistributionHistory(
          cleanCenterId,
          cleanCategoryId,
          cleanProductId,
          cleanFromDate,
          cleanToDate
        ); 

    history$.subscribe({
      next: (historyData) => {
  console.log('[DEBUG] History data for', this.viewType, historyData);
        const grouped = this.groupStockHistoryByDate(historyData);
        const sorted = Object.entries(grouped).sort((a, b) => a[0].localeCompare(b[0]));
        this.lineChartData = {
          labels: sorted.map(x => new Date(x[0]).toLocaleDateString()),
            datasets: [{
            data: sorted.map(x => x[1] as number),
            label: this.viewType === 'stock' ? 'Stock Acumulado' : 'Movimientos Acumulados',
            borderColor: '#36337f',
            backgroundColor: 'rgba(54,51,127,0.1)',
            fill: true,
            tension: 0.4
          }]
        };
      },
      error: (error) => {
        console.error('Error loading history:', error);
        this.generateStockOverTimeChartFallback();
      }
    });
  }

  shouldShowTimeChart(): boolean {
    return !!this.productId && (!this.isAdmin || !!this.centerId) && !!this.viewType;
  }

  generateIngresoEgresoChart(): void {
    const totalIngresos = this.stockData.reduce((sum, item) => sum + item.totalIngresos, 0);
    const totalEgresos = this.stockData.reduce((sum, item) => sum + item.totalEgresos, 0);
    
    this.doughnutChartData = {
      labels: ['Ingresos', 'Egresos'],
      datasets: [{
        data: [totalIngresos, totalEgresos],
        backgroundColor: ['#68d391', '#fc8181'],
        borderColor: ['#38a169', '#e53e3e'],
        borderWidth: 2
      }]
    };
  }

  clearAllFilters(): void {
    this.centerId = undefined;
    this.categoryId = undefined;
    this.productId = undefined;
    this.fromDate = undefined;
    this.toDate = undefined;
    this.loadData();
  }

  private groupSum(arr: ProductStockSummary[], keyFn: (item: ProductStockSummary) => string): Record<string, number> {
    return arr.reduce((acc, item) => {
      const key = keyFn(item);
      acc[key] = (acc[key] || 0) + Math.max(0, item.totalStock); // Only positive stock
      return acc;
    }, {} as Record<string, number>);
  }

  private groupByDate(arr: ProductStockSummary[]): Record<string, number> {
    return arr.reduce((acc, item) => {
      const date = item.lastMovementDate.split('T')[0];
      acc[date] = (acc[date] || 0) + Math.max(0, item.totalStock); // Only positive stock
      return acc;
    }, {} as Record<string, number>);
  }

  private groupStockHistoryByDate(arr: StockHistory[]): Record<string, number> {
    return arr.reduce((acc, item) => {
      const date = item.stockDate.split('T')[0];
      if (!acc[date] || acc[date] < item.stockAcumulado) {
        acc[date] = item.stockAcumulado;
      }
      return acc;
    }, {} as Record<string, number>);
  }

  private generateStockOverTimeChartFallback(): void {
    this.lineChartData = { labels: [], datasets: [] };
  }
}

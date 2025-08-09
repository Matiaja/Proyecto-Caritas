import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StockReportService, ProductStockSummary, StockHistory } from '../../services/stock-report/stock-report.service';
import { CategoryService } from '../../services/category/category.service';
import { ProductService } from '../../services/product/product.service';
import { CenterService } from '../../services/center/center.service';
import { AuthService } from '../../auth/auth.service';
import { ChartData, ChartOptions } from 'chart.js';
import { NgChartsModule } from 'ng2-charts';
import { forkJoin } from 'rxjs';

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
      }
    }
  };

  barChartOptions: ChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false }
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
      }
    }
  };

  constructor(
    private stockReportService: StockReportService,
    private categoryService: CategoryService,
    private productService: ProductService,
    private centerService: CenterService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
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
          // If centers failed to load, set empty array and continue
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
    
    // Clear undefined values to ensure proper filtering
    const cleanCenterId = this.centerId === undefined ? undefined : this.centerId;
    const cleanCategoryId = this.categoryId === undefined ? undefined : this.categoryId;
    const cleanProductId = this.productId === undefined ? undefined : this.productId;
    const cleanFromDate = this.fromDate === undefined || this.fromDate === '' ? undefined : this.fromDate;
    const cleanToDate = this.toDate === undefined || this.toDate === '' ? undefined : this.toDate;
    
    this.stockReportService.getStockReport(
      cleanCenterId,
      cleanCategoryId, 
      cleanProductId,
      cleanFromDate, 
      cleanToDate
    ).subscribe({
      next: (data) => {
        this.stockData = data;
        this.generateCharts();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading data:', error);
        this.isLoading = false;
      }
    });
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
    this.generateStockOverTimeChart();
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
    // Usar el servicio de histórico para obtener datos más precisos
    const cleanCenterId = this.centerId === undefined ? undefined : this.centerId;
    const cleanCategoryId = this.categoryId === undefined ? undefined : this.categoryId;
    const cleanProductId = this.productId === undefined ? undefined : this.productId;
    const cleanFromDate = this.fromDate === undefined || this.fromDate === '' ? undefined : this.fromDate;
    const cleanToDate = this.toDate === undefined || this.toDate === '' ? undefined : this.toDate;
    
    this.stockReportService.getStockHistory(
      cleanCenterId,
      cleanCategoryId,
      cleanProductId,
      cleanFromDate,
      cleanToDate
    ).subscribe({
      next: (historyData) => {
        const grouped = this.groupStockHistoryByDate(historyData);
        const sorted = Object.entries(grouped).sort((a, b) => a[0].localeCompare(b[0]));
        
        this.lineChartData = {
          labels: sorted.map(x => new Date(x[0]).toLocaleDateString()),
          datasets: [{
            data: sorted.map(x => x[1] as number),
            label: 'Stock Acumulado',
            borderColor: '#36337f',
            backgroundColor: 'rgba(54, 51, 127, 0.1)',
            fill: true,
            tension: 0.4
          }]
        };
      },
      error: (error) => {
        console.error('Error loading stock history:', error);
        // Fallback al método original si falla
        this.generateStockOverTimeChartFallback();
      }
    });
  }

  generateStockOverTimeChartFallback(): void {
    const grouped = this.groupByDate(this.stockData);
    const sorted = Object.entries(grouped).sort((a, b) => a[0].localeCompare(b[0]));
    
    this.lineChartData = {
      labels: sorted.map(x => new Date(x[0]).toLocaleDateString()),
      datasets: [{
        data: sorted.map(x => x[1] as number),
        label: 'Stock Total',
        borderColor: '#36337f',
        backgroundColor: 'rgba(54, 51, 127, 0.1)',
        fill: true,
        tension: 0.4
      }]
    };
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
    // Agrupar por fecha y tomar el stock acumulado más reciente para cada fecha
    const grouped = arr.reduce((acc, item) => {
      const date = item.stockDate.split('T')[0];
      if (!acc[date] || acc[date] < item.stockAcumulado) {
        acc[date] = item.stockAcumulado;
      }
      return acc;
    }, {} as Record<string, number>);
    
    return grouped;
  }
}

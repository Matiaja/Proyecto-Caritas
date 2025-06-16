import { Component, OnDestroy, OnInit, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { ResponsiveService } from '../../services/responsive/responsive.service';
import { GlobalStateService } from '../../services/global/global-state.service';
import { Subscription } from 'rxjs';
import { AuthService } from '../../auth/auth.service';
import { CenterService } from '../../services/center/center.service';
import { CategoryService } from '../../services/category/category.service';
import { StockReportService, ChartData } from '../../services/stock-report/stock-report.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
})
export class HomeComponent implements OnInit, OnDestroy, AfterViewInit {
  @ViewChild('categoryChart') categoryChart!: ElementRef<HTMLCanvasElement>;
  @ViewChild('typeChart') typeChart!: ElementRef<HTMLCanvasElement>;
  @ViewChild('monthChart') monthChart!: ElementRef<HTMLCanvasElement>;
  @ViewChild('productsChart') productsChart!: ElementRef<HTMLCanvasElement>;

  isMobile: boolean = false;
  private resizeSub!: Subscription;
  isAdmin = false;
  centers: any[] = [];
  categories: any[] = [];
  products: any[] = [];
  selectedCenterId: number | string = '';
  selectedCategory: string = '';
  selectedProduct: string = '';
  dateFrom: string = '';
  dateTo: string = '';
  loading = false;
  error = '';

  private charts: { [key: string]: Chart } = {};
  private allChartData: ChartData | null = null;

  constructor(
    private responsiveService: ResponsiveService,
    private globalStateService: GlobalStateService,
    private authService: AuthService,
    private centerService: CenterService,
    private categoryService: CategoryService,
    private stockReportService: StockReportService
  ) {}

  ngOnInit() {
    this.loading = true; // Iniciar loading desde el principio
    
    this.resizeSub = this.responsiveService.isMobile$.subscribe((isMobile) => {
      this.isMobile = isMobile;
    });

    this.isAdmin = this.authService.isAdmin();
    this.loadInitialData();
  }

  ngAfterViewInit() {
    setTimeout(() => {
      if (this.selectedCenterId) {
        this.loadChartData();
      }
    }, 100);
  }

  loadInitialData() {
    // Load categories
    this.categoryService.getAllCategories().subscribe({
      next: (categories) => {
        this.categories = categories;
      },
      error: (error) => console.error('Error loading categories:', error)
    });

    if (this.isAdmin) {
      this.loadCenters();
    } else {
      const userCenterId = this.authService.getUserCenterId?.() ?? this.globalStateService.getCurrentCenterId();
      if (userCenterId) {
        this.selectedCenterId = userCenterId;
        this.loadProducts();
      }
    }
  }

  loadCenters() {
    this.centerService.getCenters().subscribe({
      next: (centers) => {
        this.centers = centers;
        if (centers.length > 0 && !this.selectedCenterId) {
          if (centers[0].id !== undefined) {
            this.selectedCenterId = centers[0].id;
            this.loadProducts();
            this.loadChartData();
          }
        }
      },
      error: (error) => {
        console.error('Error loading centers:', error);
        this.error = 'Error al cargar los centros';
      }
    });
  }

  loadProducts() {
    if (!this.selectedCenterId) return;

    this.stockReportService.getProductsByCenter(Number(this.selectedCenterId)).subscribe({
      next: (products) => {
        this.products = products;
      },
      error: (error) => console.error('Error loading products:', error)
    });
  }

  onCenterChange() {
    if (this.selectedCenterId) {
      this.clearFilters();
      this.loadProducts();
      this.loadChartData();
    }
  }

  applyFilters() {
    if (!this.allChartData) return;

    this.loading = true;
    
    setTimeout(() => {
      const filteredData = this.filterChartData(this.allChartData!);
      this.createCharts(filteredData);
      this.loading = false;
    }, 300);
  }

  clearFilters() {
    this.selectedCategory = '';
    this.selectedProduct = '';
    this.dateFrom = '';
    this.dateTo = '';
    
    if (this.allChartData) {
      this.createCharts(this.allChartData);
    }
  }

  filterChartData(data: ChartData): ChartData {
    let filteredData = { ...data };

    // Filter by category - aplicar a todos los gráficos relacionados con categorías
    if (this.selectedCategory) {
      const categoryName = this.categories.find(c => c.id == this.selectedCategory)?.name;
      if (categoryName) {
        // Filtrar stock por categoría
        filteredData.stockByCategory = data.stockByCategory.filter(item => 
          item.categoryName === categoryName
        );
        
        // Filtrar productos top que pertenezcan a la categoría seleccionada
        // Necesitamos obtener los productos de esa categoría desde this.products
        const productsInCategory = this.products.filter(p => {
          // Buscar la categoría del producto
          return this.categories.find(c => c.id == this.selectedCategory)?.name === categoryName;
        });
        
        if (productsInCategory.length > 0) {
          const productNamesInCategory = productsInCategory.map(p => p.productName);
          filteredData.topProducts = data.topProducts.filter(item => 
            productNamesInCategory.includes(item.productName)
          );
        } else {
          filteredData.topProducts = [];
        }
      }
    }

    // Filter by product - aplicar a todos los gráficos relacionados con productos
    if (this.selectedProduct) {
      const selectedProductData = this.products.find(p => p.productId == this.selectedProduct);
      if (selectedProductData) {
        // Filtrar solo el producto seleccionado en top products
        filteredData.topProducts = data.topProducts.filter(item => 
          item.productName === selectedProductData.productName
        );
        
        // Filtrar categoría del producto seleccionado
        const productCategoryId = selectedProductData.categoryId;
        if (productCategoryId) {
          const categoryName = this.categories.find(c => c.id === productCategoryId)?.name;
          if (categoryName) {
            filteredData.stockByCategory = data.stockByCategory.filter(item => 
              item.categoryName === categoryName
            );
          }
        }
      }
    }

    // Filter by date range - aplicar a gráficos con datos temporales
    if (this.dateFrom || this.dateTo) {
      filteredData.stockByMonth = data.stockByMonth.filter(item => {
        const itemDate = new Date(item.month + '-01');
        const fromDate = this.dateFrom ? new Date(this.dateFrom) : new Date('1900-01-01');
        const toDate = this.dateTo ? new Date(this.dateTo) : new Date('2100-12-31');
        
        return itemDate >= fromDate && itemDate <= toDate;
      });
    }

    return filteredData;
  }

  loadChartData() {
    if (!this.selectedCenterId) {
      this.error = 'No hay centro seleccionado';
      return;
    }

    this.loading = true;
    this.error = '';

    const centerId = Number(this.selectedCenterId);

    this.stockReportService.getChartData(centerId).subscribe({
      next: (data) => {
        this.allChartData = data;
        this.createCharts(data);
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading chart data:', error);
        this.error = 'Error al cargar los datos de los gráficos';
        this.loading = false;
      }
    });
  }

  createCharts(data: ChartData) {
    // Verificar que tenemos los canvas elements
    if (!this.categoryChart || !this.typeChart || !this.monthChart || !this.productsChart) {
      setTimeout(() => this.createCharts(data), 100);
      return;
    }

    this.createCategoryChart(data.stockByCategory);
    this.createTypeChart(data.stockByType);
    this.createMonthChart(data.stockByMonth);
    this.createProductsChart(data.topProducts);
  }

  createCategoryChart(data: any[]) {
    if (this.charts['category']) {
      this.charts['category'].destroy();
    }

    const ctx = this.categoryChart.nativeElement.getContext('2d');
    if (!ctx) return;

    this.charts['category'] = new Chart(ctx, {
      type: 'pie',
      data: {
        labels: data.map(item => item.categoryName),
        datasets: [{
          data: data.map(item => item.totalStock),
          backgroundColor: [
            '#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0',
            '#9966FF', '#FF9F40', '#FF6B8A', '#C9CBCF'
          ]
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: 'bottom',
            labels: {
              font: {
                size: this.isMobile ? 10 : 12
              }
            }
          },
          tooltip: {
            callbacks: {
              label: function(context) {
                const label = context.label || '';
                const value = context.parsed || 0;
                return `${label}: ${value} unidades`;
              }
            }
          }
        }
      }
    });
  }

  createTypeChart(data: any[]) {
    if (this.charts['type']) {
      this.charts['type'].destroy();
    }

    const ctx = this.typeChart.nativeElement.getContext('2d');
    if (!ctx) return;

    this.charts['type'] = new Chart(ctx, {
      type: 'doughnut',
      data: {
        labels: data.map(item => item.stockType),
        datasets: [{
          data: data.map(item => item.totalStock),
          backgroundColor: ['#36A2EB', '#FF6384', '#FFCE56', '#4BC0C0']
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: 'bottom',
            labels: {
              font: {
                size: this.isMobile ? 10 : 12
              }
            }
          },
          tooltip: {
            callbacks: {
              label: function(context) {
                const label = context.label || '';
                const value = context.parsed || 0;
                return `${label}: ${value} unidades`;
              }
            }
          }
        }
      }
    });
  }

  createMonthChart(data: any[]) {
    if (this.charts['month']) {
      this.charts['month'].destroy();
    }

    const ctx = this.monthChart.nativeElement.getContext('2d');
    if (!ctx) return;

    this.charts['month'] = new Chart(ctx, {
      type: 'line',
      data: {
        labels: data.map(item => {
          const [year, month] = item.month.split('-');
          return `${month}/${year}`;
        }),
        datasets: [{
          label: 'Stock Total',
          data: data.map(item => item.totalStock),
          borderColor: '#36A2EB',
          backgroundColor: 'rgba(54, 162, 235, 0.1)',
          fill: true,
          tension: 0.4
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        scales: {
          y: {
            beginAtZero: true,
            title: {
              display: !this.isMobile,
              text: 'Cantidad'
            },
            ticks: {
              font: {
                size: this.isMobile ? 10 : 12
              }
            }
          },
          x: {
            title: {
              display: !this.isMobile,
              text: 'Mes'
            },
            ticks: {
              font: {
                size: this.isMobile ? 10 : 12
              },
              maxRotation: this.isMobile ? 45 : 0
            }
          }
        },
        plugins: {
          legend: {
            display: !this.isMobile
          },
          tooltip: {
            callbacks: {
              label: function(context) {
                const value = context.parsed.y || 0;
                return `Stock: ${value} unidades`;
              }
            }
          }
        }
      }
    });
  }

  createProductsChart(data: any[]) {
    if (this.charts['products']) {
      this.charts['products'].destroy();
    }

    const ctx = this.productsChart.nativeElement.getContext('2d');
    if (!ctx) return;

    this.charts['products'] = new Chart(ctx, {
      type: 'bar',
      data: {
        labels: data.map(item => this.isMobile ? 
          item.productName.substring(0, 15) + (item.productName.length > 15 ? '...' : '') : 
          item.productName
        ),
        datasets: [{
          label: 'Cantidad',
          data: data.map(item => item.totalStock),
          backgroundColor: '#4BC0C0'
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        scales: {
          y: {
            beginAtZero: true,
            title: {
              display: !this.isMobile,
              text: 'Cantidad'
            },
            ticks: {
              font: {
                size: this.isMobile ? 10 : 12
              }
            }
          },
          x: {
            ticks: {
              maxRotation: this.isMobile ? 90 : 45,
              minRotation: this.isMobile ? 45 : 0,
              font: {
                size: this.isMobile ? 9 : 11
              }
            },
            title: {
              display: !this.isMobile,
              text: 'Productos'
            }
          }
        },
        plugins: {
          legend: {
            display: !this.isMobile
          },
          tooltip: {
            callbacks: {
              label: function(context) {
                const value = context.parsed.y || 0;
                return `Stock: ${value} unidades`;
              }
            }
          }
        }
      }
    });
  }

  ngOnDestroy() {
    if (this.resizeSub) {
      this.resizeSub.unsubscribe();
    }
    
    // Destroy all charts
    Object.values(this.charts).forEach(chart => {
      if (chart) {
        chart.destroy();
      }
    });
  }
}

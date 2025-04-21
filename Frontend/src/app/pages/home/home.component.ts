import { Component, OnDestroy, OnInit } from '@angular/core';
import { NgChartsModule } from 'ng2-charts';
import { ChartOptions } from 'chart.js';
import { Subscription } from 'rxjs';
import { ResponsiveService } from '../../services/responsive/responsive.service';
import { StockService } from '../../services/stock/stock.service';
import { GlobalStateService } from '../../services/global/global-state.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [NgChartsModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
})
export class HomeComponent implements OnInit, OnDestroy {
  isMobile: boolean = false;
  private resizeSub!: Subscription;
  topProductsData: any;
  stockByCategoryData: any;

  constructor(private responsiveService: ResponsiveService, private stockService: StockService, private globalStateService: GlobalStateService) {}
  ngOnInit() {
    this.resizeSub = this.responsiveService.isMobile$.subscribe((isMobile) => {
      this.isMobile = isMobile;
    });

    const centerId = this.globalStateService.getCurrentCenterId();
    this.stockService.getMoreQuantityProducts(centerId).subscribe((products) => {
      this.prepareProductChart(products);
    });

    this.stockService.getStockByCategory(centerId).subscribe((categories) => {
      this.prepareCategoryChart(categories);
    });
  }
  ngOnDestroy() {
    if (this.resizeSub) {
      this.resizeSub.unsubscribe();
    }
  }

  prepareProductChart(products: any[]) {
    this.topProductsData = {
      labels: products.map((product) => product.productName),
      datasets: [
        {
          label: 'Unidades en stock',
          data: products.map((product) => product.stockQuantity),
          backgroundColor: ['#5ba4f1', '#66BB6A', '#FFA726', '#42A5F5', '#8e44ad'],
        },
      ],
    };
  }

  prepareCategoryChart(categories: any[]) {
    this.stockByCategoryData = {
      labels: categories.map((category) => category.categoryName),
      datasets: [
        {
          label: 'Stock por categoría',
          data: categories.map((category) => category.totalStock),
          backgroundColor: ['#5ba4f1', '#a29bfe', '#74b9ff', '#00cec9'],
        },
      ],
    };
  }

  // 3. Entradas vs. Salidas (último mes)
  movementData = {
    labels: ['Entradas', 'Salidas'],
    datasets: [
      {
        label: 'Movimientos de stock',
        data: [400, 275],
        backgroundColor: ['#66BB6A', '#ff6b6b'],
      },
    ],
  };

  chartOptions: ChartOptions = {
    responsive: true,
    plugins: {
      legend: {
        labels: {
          color: '#686a74',
        },
      },
    },
  };
}

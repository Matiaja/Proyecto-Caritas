import { Component, OnDestroy, OnInit } from '@angular/core';
import { NgChartsModule } from 'ng2-charts';
import { ChartOptions } from 'chart.js';
import { Subscription } from 'rxjs';
import { ResponsiveService } from '../../services/responsive/responsive.service';

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

  constructor(private responsiveService: ResponsiveService) {}
  ngOnInit() {
    this.resizeSub = this.responsiveService.isMobile$.subscribe((isMobile) => {
      this.isMobile = isMobile;
    });
  }
  ngOnDestroy() {
    if (this.resizeSub) {
      this.resizeSub.unsubscribe();
    }
  }
  // 1. Top productos con más stock
  topProductsData = {
    labels: ['Arroz', 'Fideos', 'Aceite', 'Harina', 'Lentejas'],
    datasets: [
      {
        label: 'Unidades en stock',
        data: [250, 200, 180, 160, 140],
        backgroundColor: ['#5ba4f1', '#66BB6A', '#FFA726', '#42A5F5', '#8e44ad'],
      },
    ],
  };

  // 2. Stock por categoría
  stockByCategoryData = {
    labels: ['Alimentos', 'Higiene', 'Ropa', 'Medicamentos'],
    datasets: [
      {
        label: 'Stock por categoría',
        data: [700, 300, 250, 120],
        backgroundColor: ['#5ba4f1', '#a29bfe', '#74b9ff', '#00cec9'],
      },
    ],
  };

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

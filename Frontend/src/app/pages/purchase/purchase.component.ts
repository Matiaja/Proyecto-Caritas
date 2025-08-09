import { Component, OnInit } from '@angular/core';
import { PurchaseService } from '../../services/purchase/purchase.service';
import { Router } from '@angular/router';
import { UiTableComponent } from "../../shared/components/ui-table/ui-table.component";
import { ResponsiveService } from '../../services/responsive/responsive.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-purchase',
  standalone: true,
  imports: [UiTableComponent, CommonModule],
  templateUrl: './purchase.component.html',
  styleUrl: './purchase.component.css'
})
export class PurchaseComponent implements OnInit {
  purchases: any[] = [];
  title = 'Compras';
  columnHeaders: Record<string, string> = {
    purchaseDate: 'Fecha',
    type: 'Tipo',
    centerName: 'Centro',
  };
  displayedColumns = ['purchaseDate', 'type', 'centerName'];
  mobileHeaders: Record<string, string> = {
    purchaseDate: 'Fecha',
    type: 'Tipo',
    centerName: 'Centro',
  };
  mobileColumns = ['purchaseDate', 'type', 'centerName'];

  isMobile = false;

  constructor(
    private purchaseService: PurchaseService, 
    private router: Router,
    private responsiveService: ResponsiveService
  ) {
    this.responsiveService.isMobile$.subscribe((isMobile) => {
      this.isMobile = isMobile;
    });
  }

  ngOnInit(): void {
    this.purchaseService.getAll().subscribe(purchases => {
      this.purchases = purchases.map(purchase => ({
        ...purchase,
        purchaseDate: new Date(purchase.purchaseDate).toLocaleDateString('es-ES', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
          })
      }));
    });
  }

  onAddPurchase() {
    this.router.navigate(['/purchases/add']);
  }

  viewDetails(purchase: any): void {
    this.router.navigate(['/purchases', purchase.id]);
  }

  startDistribution(purchase: any): void {
    this.router.navigate(['/purchases', purchase.id, 'distribute']);
  }
}

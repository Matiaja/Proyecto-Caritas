import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { PurchaseService } from '../../../services/purchase/purchase.service';
import { CommonModule } from '@angular/common';
import { UiTableComponent } from "../../../shared/components/ui-table/ui-table.component";

@Component({
  selector: 'app-purchase-detail',
  standalone: true,
  imports: [CommonModule, UiTableComponent],
  templateUrl: './purchase-detail.component.html',
  styleUrl: './purchase-detail.component.css'
})

export class PurchaseDetailComponent implements OnInit {
  purchaseId!: number;
  purchase!: any;
  items: any[] = [];
  expandedDistId: number | null = null;

  // tabla items
  displayedColumns = ['productName', 'quantity', 'remainingQuantity'];
  columnHeaders = {
    productName: 'Producto',
    quantity: 'Cantidad',
    remainingQuantity: 'Restante'
  };
  title = 'Detalle de la compra';
  mobileColumns = ['productName', 'quantity', 'remainingQuantity'];
  mobileHeaders = {
    productName: 'Producto',
    quantity: 'Cant.',
    remainingQuantity: 'Restante'
  };

  constructor(private route: ActivatedRoute, private purchaseService: PurchaseService) {}

  ngOnInit(): void {
    this.purchaseId = Number(this.route.snapshot.paramMap.get('id'));
    this.purchaseService.getById(this.purchaseId).subscribe(p => {
      this.purchase = p;
      this.items = p.items;
      console.log('Detalles de la compra:', this.purchase);
    });
  }

  toggleDetails(distId: number): void {
    this.expandedDistId = this.expandedDistId === distId ? null : distId;
  }
}

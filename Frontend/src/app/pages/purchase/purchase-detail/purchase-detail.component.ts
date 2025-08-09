import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { PurchaseService } from '../../../services/purchase/purchase.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-purchase-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './purchase-detail.component.html',
  styleUrl: './purchase-detail.component.css'
})

export class PurchaseDetailComponent implements OnInit {
  purchaseId!: number;
  purchase!: any;
  items: any[] = [];
  expandedDistId: number | null = null;

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

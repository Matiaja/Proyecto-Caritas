import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PurchaseService } from '../../../services/purchase/purchase.service';
import { DistributionService } from '../../../services/distribution/distribution.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CenterService } from '../../../services/center/center.service';

@Component({
  selector: 'app-distribution',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './distribution.component.html',
  styleUrl: './distribution.component.css'
})
export class DistributionComponent implements OnInit {
  purchaseId!: number;
  purchase!: any;

  destinationType: 'center' | 'person' = 'center';
  destinationValue = '';

  items: { item: any; quantityToDeliver: number }[] = [];

  centers: any[] = [];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private purchaseService: PurchaseService,
    private distributionService: DistributionService,
    private centerService: CenterService
  ) {}

  ngOnInit(): void {
    this.purchaseId = Number(this.route.snapshot.paramMap.get('id'));
    this.purchaseService.getById(this.purchaseId).subscribe(p => {
      this.purchase = p;
      this.items = p.items.map((i: any) => ({ item: i, quantityToDeliver: 0 }));
    });
    // agregar centers
    this.centerService.getCenters().subscribe(centers => {
      this.centers = centers;
    });
  }

  confirmDistribution(): void {
    const distribution: any = {
      purchaseId: this.purchaseId,
      centerId: this.destinationType === 'center' ? Number(this.destinationValue) : null,
      personName: this.destinationType === 'person' ? this.destinationValue : null,
      items: this.items
        .filter(i => i.quantityToDeliver > 0)
        .map(i => ({
          itemPurchaseId: i.item.id,
          quantity: i.quantityToDeliver
        }))
    };

    this.distributionService.create(distribution).subscribe(() => {
      this.router.navigate(['/purchases']);
    });
  }

  cancel(): void {
    this.router.navigate(['/purchases']);
  }
}

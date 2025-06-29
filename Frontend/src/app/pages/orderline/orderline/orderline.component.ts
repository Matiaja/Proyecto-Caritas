import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { OrderLineService } from '../../../services/order-line/order-line.service';
import { OrderLine } from '../../../models/orderLine.model';
import { CommonModule } from '@angular/common';
import { BreadcrumbComponent } from "../../../shared/components/breadcrumbs/breadcrumbs.component";
import { UiTableComponent } from "../../../shared/components/ui-table/ui-table.component";

@Component({
  selector: 'app-orderline',
  standalone: true,
  imports: [CommonModule, BreadcrumbComponent, UiTableComponent],
  templateUrl: './orderline.component.html',
  styleUrl: './orderline.component.css'
})
export class OrderlineComponent  implements OnInit {

  orderLineId = 0;
  orderLine: OrderLine | null = null;
  assignedQuantity = 0;
  pendingQuantity = 0;

  // Variables para la tabla
  title = 'Donaciones asignadas';
  columnHeaders: Record<string, string> = {
    status: 'Estado',
    centerName: 'Centro',
    quantity: 'Cantidad',
  };
  mobileHeaders: Record<string, string> = {
    status: 'Estado',
    centerName: 'Centro',
    quantity: 'Cantidad',
  };
  displayedColumns = ['status', 'centerName', 'quantity'];
  mobileColumns = ['status', 'centerName', 'quantity'];
  donationsData: any[] = [];

  constructor(
    private route: ActivatedRoute, 
    private orderLineService: OrderLineService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.orderLineId = +params['id'];
      this.orderLineService.getOrderLineById(this.orderLineId).subscribe({
        next: (data) => {
          this.orderLine = data;
          this.assignedQuantity = data.donationRequests?.reduce((sum: number, dr) => sum + dr.quantity, 0) ?? 0;
          this.pendingQuantity = data.quantity - this.assignedQuantity;
          
          // Asignar los datos a donationsData para la tabla
          this.donationsData = (data.donationRequests || []).map(dr => ({
            centerName: dr.assignedCenter?.name || 'Sin nombre',
            quantity: dr.quantity,
            status: dr.status,
          }));
        },
        error: (error) => console.error(error)
      });
    });
  }

  goBack() {
    this.router.navigate(['/requests', this.orderLine?.requestId]);
  }

  onAddElement = null;
  onEditElement = null;
  onDeleteElement = null;
  onSelectElement = null;
}

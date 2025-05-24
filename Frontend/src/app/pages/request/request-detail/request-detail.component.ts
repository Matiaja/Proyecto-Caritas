import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { RequestService } from '../../../services/request/request.service';
import { RequestModel } from '../../../models/request.model';
import { ProductService } from '../../../services/product/product.service';
import { UiTableComponent } from '../../../shared/components/ui-table/ui-table.component';
import { CommonModule, Location } from '@angular/common';

@Component({
  selector: 'app-request-detail',
  standalone: true,
  imports: [UiTableComponent, CommonModule],
  templateUrl: './request-detail.component.html',
  styleUrl: './request-detail.component.css',
})
export class RequestDetailComponent implements OnInit {
  request: RequestModel = {
    id: 0,
    requestingCenterId: 0,
    urgencyLevel: '',
    requestDate: '',
    requestingCenter: {
      id: 0,
      name: '',
    },
    orderLines: [],
  } as RequestModel;

  title = 'Lista de pedidos';
  columnHeaders: Record<string, string> = {
    id: 'ID',
    productName: 'Producto',
    quantity: 'Cantidad',
    description: 'Descripción',
    isAssigned: 'Asignado',
  };
  displayedColumns = ['id', 'productName', 'quantity', 'description', 'isAssigned'];
  orderLines: any[] = [];

  constructor(
    private requestService: RequestService,
    private productService: ProductService,
    private route: ActivatedRoute,
    private router: Router,
    private location: Location
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      const reqId = params['id'];
      this.loadRequestDetails(reqId);
    });
  }

  loadRequestDetails(reqId: any): void {
    this.requestService.getRequestById(reqId).subscribe({
      next: (req: RequestModel) => {
        this.request = req;
        console.log(this.request);
        this.loadOrderLines();
      },
      error: (err) => {
        console.log(err);
      },
    });
  }

  loadOrderLines(): void {
    this.orderLines = this.request.orderLines.map((line) => {
      const orderLine = {
        ...line,
        isAssigned: line.donationRequestId !== null ? 'Sí' : 'No',
        productName: '',
      };
      this.productService.getProductById(line.productId).subscribe({
        next: (product) => {
          orderLine.productName = product.name;
        },
        error: () => {
          orderLine.productName = 'Producto no encontrado';
        },
      });
      return orderLine;
    });
  }

  editRequest() {
    throw new Error('Method not implemented.');
  }

  assign(row: any): void {
    this.router.navigate(['/requests', this.request.id, 'assign'], {
      queryParams: { orderLineId: row.id, productId: row.productId },
    });
  }

  onSelectOrderLine(ol: any) {
    console.log('Select order line', ol);
  }

  goBack() {
    this.location.back();
  }

  onAddElement = null;
  onEditElement = null;
  onDeleteElement = null;
}

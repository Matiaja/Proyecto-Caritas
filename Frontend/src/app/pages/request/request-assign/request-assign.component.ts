import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductService } from '../../../services/product/product.service';
import { Product } from '../../../models/product.model';
import { CommonModule, Location } from '@angular/common';
import { RequestModel } from '../../../models/request.model';
import { RequestService } from '../../../services/request/request.service';
import { OrderLine } from '../../../models/orderLine.model';
import { UiTableComponent } from '../../../shared/components/ui-table/ui-table.component';
import { StockService } from '../../../services/stock/stock.service';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { DonationRequestService } from '../../../services/donationRequest/donation-request.service';
import { DonationRequest } from '../../../models/donationRequest.model';
import { BreadcrumbComponent } from '../../../shared/components/breadcrumbs/breadcrumbs.component';

@Component({
  selector: 'app-request-assign',
  standalone: true,
  imports: [UiTableComponent, CommonModule, BreadcrumbComponent, FormsModule],
  templateUrl: './request-assign.component.html',
  styleUrl: './request-assign.component.css',
})
export class RequestAssignComponent implements OnInit {
  constructor(
    private route: ActivatedRoute,
    private productService: ProductService,
    private requestService: RequestService,
    private stockService: StockService,
    private donationRequestService: DonationRequestService,
    private location: Location,
    private toastr: ToastrService,
    private router: Router,
  ) {}

  // parametros
  productId: number | null = null;
  orderLineId: number | null = null;
  requestId: number | null = null;
  //variables
  product: Product = {} as Product;
  request: RequestModel = {
    id: 0,
    requestingCenterId: 0,
    urgencyLevel: '',
    requestDate: '',
    status: '',
    requestingCenter: {
      id: 0,
      name: '',
    },
    orderLines: [],
  } as RequestModel;
  orderLine: OrderLine = {} as OrderLine;
  pendingQuantity = 0;
  // variables de tabla
  title = '';
  columnHeaders: Record<string, string> = {
    productName: 'Producto',
    centerName: 'Centro',
    stockQuantity: 'Cantidad',
  };
  displayedColumns = ['productName', 'centerName', 'stockQuantity'];
  stocks: any[] = [];

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      this.requestId = +params['id'];
      this.orderLineId = +params['idorderline'];
    });

    if (this.requestId && this.orderLineId) {
      this.loadRequest(this.requestId);
    }
  }

  loadRequest(reqId: number) {
    this.requestService.getRequestById(reqId).subscribe({
      next: (request: RequestModel) => {
        this.request = request;
        const ol = request.orderLines.find((line) => line.id === this.orderLineId);
        if (ol) {
          this.orderLine = ol;
          const assigned = ol.donationRequests?.reduce((sum, dr) => sum + dr.quantity, 0) ?? 0;
          this.pendingQuantity = ol.quantity - assigned;
        }
        this.loadProduct();
      },
      error: (error) => {
        console.error(error);
      },
    });
  }
  loadProduct() {
    if (this.orderLineId) {
      this.productId = this.orderLine.productId;
      this.searchProductInStock(this.productId);
    }
  }

  searchProductInStock(productId: number | null) {
    if (productId) {
      this.productService.getProductById(productId).subscribe({
        next: (product) => {
          this.product = product;
          this.title = `Lista de ${this.product.name} en stocks`;
          if (this.product) {
            this.loadStocks();
          }
        },
        error: (error) => {
          console.error(error);
        },
      });
    }
  }

  loadStocks() {
    if (this.product && this.product.stocks && this.request && this.request.requestingCenter) {
      this.stockService.getProductInStocks(this.product.id).subscribe({
        next: (stocks) => {
          this.stocks = stocks.map(stock => ({
            ...stock,
            assignQuantity: this.getMaxAssignableQuantity(stock) // Inicializar con el valor máximo
          }));
        },
        error: (error) => {
          console.error(error);
        },
      });
    }
  }

  goBack() {
    this.location.back();
  }

  assign(row: any): void {
    const quantity = Number(row.assignQuantity);

    if (!quantity || quantity <= 0) {
      this.toastr.warning('Ingresá una cantidad válida.');
      return;
    }

    if (quantity > this.getMaxAssignableQuantity(row)) {
      this.toastr.warning('La cantidad excede el máximo permitido.');
      return;
    }

    const donationRequest: DonationRequest = {
      orderLineId: this.orderLineId!,
      assignedCenterId: row.centerId,
      quantity: quantity,
      status: 'Asignada'
    };
    
    this.donationRequestService.addDonationRequest(donationRequest).subscribe({
      next: () => {
        this.toastr.success('Centro asignado correctamente.');
        this.router.navigate(['/orderline', this.orderLineId]);
      },
      error: (err) => {
        this.toastr.error(err.error.message || 'Error al asignar el centro.');
      }
    });
  }

  getMaxAssignableQuantity(row: any): number {
    return Math.min(row.stockQuantity, this.pendingQuantity);
  }

  onAddElement = null;
  onEditElement = null;
  onDeleteElement = null;
  onSelectElement = null;
}

import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ProductService } from '../../../services/product/product.service';
import { Product } from '../../../models/product.model';
import { CommonModule, Location } from '@angular/common';
import { RequestModel } from '../../../models/request.model';
import { RequestService } from '../../../services/request/request.service';
import { OrderLine } from '../../../models/orderLine.model';
import { UiTableComponent } from '../../../shared/components/ui-table/ui-table.component';
import { BreadcrumbComponent } from '../../../shared/components/breadcrumbs/breadcrumbs.component';
import { StockService } from '../../../services/stock/stock.service';

@Component({
  selector: 'app-request-assign',
  standalone: true,
  imports: [UiTableComponent, CommonModule, BreadcrumbComponent],
  templateUrl: './request-assign.component.html',
  styleUrl: './request-assign.component.css',
})
export class RequestAssignComponent implements OnInit {
  constructor(
    private route: ActivatedRoute,
    private productService: ProductService,
    private requestService: RequestService,
    private stockService: StockService,
    private location: Location
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
    requestingCenter: {
      id: 0,
      name: '',
    },
    orderLines: [],
  } as RequestModel;
  orderLine: OrderLine = {} as OrderLine;
  // variables de tabla
  title = '';
  columnHeaders: { [key: string]: string } = {
    productName: 'Producto',
    centerName: 'Centro',
    stockQuantity: 'Cantidad',
  };
  displayedColumns = ['productName', 'centerName', 'stockQuantity'];
  stocks: any[] = [];

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      this.requestId = params['id'];
      if (this.requestId) {
        this.loadRequest(this.requestId);
      }
    });
  }

  loadRequest(reqId: number) {
    this.requestService.getRequestById(reqId).subscribe({
      next: (request: RequestModel) => {
        this.request = request;
        const ol = request.orderLines.find((line) => line.id === this.orderLineId);
        if (ol) {
          this.orderLine = ol;
        }
        this.loadProduct();
      },
      error: (error) => {
        console.error(error);
      },
    });
  }
  loadProduct() {
    this.route.queryParams.subscribe((queryParams) => {
      this.productId = queryParams['productId'] ? Number(queryParams['productId']) : null;
      this.orderLineId = queryParams['orderLineId'] ? Number(queryParams['orderLineId']) : null;

      if (this.productId && this.orderLineId) {
        this.searchProductInStock(this.productId);
      }
    });
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
          this.stocks = stocks;
          console.log(stocks);
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
    console.log(row);
  }

  onAddElement = null;
  onEditElement = null;
  onDeleteElement = null;
  onSelectElement = null;
}

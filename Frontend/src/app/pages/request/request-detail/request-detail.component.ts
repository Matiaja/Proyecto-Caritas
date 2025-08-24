import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { RequestService } from '../../../services/request/request.service';
import { RequestModel } from '../../../models/request.model';
import { ProductService } from '../../../services/product/product.service';
import { UiTableComponent } from '../../../shared/components/ui-table/ui-table.component';
import { CommonModule, Location } from '@angular/common';
import { ResponsiveService } from '../../../services/responsive/responsive.service';
import { PdfService } from '../../../services/pdf/pdf.service';
import { AuthService } from '../../../auth/auth.service';

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
    status: '',
    requestingCenter: {
      id: 0,
      name: '',
    },
    orderLines: [],
  } as RequestModel;

  title = 'Lista de pedidos';
  columnHeaders: Record<string, string> = {
    id: '#',
    productName: 'Producto',
    quantity: 'Cantidad',
    description: 'Descripción',
    status: 'Estado',
  };
  mobileHeaders: Record<string, string> = {
    id: '#',
    productName: 'Producto',
    quantity: 'Cant.',
    status: 'Estado',
  };
  displayedColumns = ['id', 'productName', 'quantity', 'description', 'status'];
  mobileColumns = ['id', 'productName', 'quantity', 'status'];
  orderLines: any[] = [];

  isMobile = false;
  isAdmin = false;

  constructor(
    private requestService: RequestService,
    private productService: ProductService,
    private route: ActivatedRoute,
    private router: Router,
    private location: Location,
    private responsiveService: ResponsiveService,
    private pdfService: PdfService,
    private authService: AuthService
  ) {
    this.responsiveService.isMobile$.subscribe((isMobile) => {
      this.isMobile = isMobile;
    });
    this.isAdmin = this.authService.isAdmin();
  }

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
    this.router.navigate(['/requests', this.request.id, 'assign', row.id]);
  }

  onSelectOrderLine(ol: any) {
    this.router.navigate(['/orderline', ol.id]);
  }

  goBack() {
    this.router.navigate(['/requests']);
  }

  generateRequestPdf(): void {
    if (!this.request || !this.request.id) {
      console.error('No hay datos de solicitud para generar PDF');
      return;
    }

    // Preparar los datos con los nombres de productos ya cargados
    const requestDataForPdf = {
      ...this.request,
      orderLines: this.orderLines, // Usar orderLines que ya tiene productName
    };

    this.pdfService.generateRequestDetailPdfWithData(requestDataForPdf).subscribe({
      next: (blob) => {
        const filename = `solicitud_${this.request.id}_${new Date().toISOString().split('T')[0]}.pdf`;
        this.pdfService.downloadPdf(blob, filename);
      },
      error: (error) => {
        console.error('Error generating PDF:', error);
      },
    });
  }

  onAddElement = null;
  onEditElement = null;
  onDeleteElement = null;
}

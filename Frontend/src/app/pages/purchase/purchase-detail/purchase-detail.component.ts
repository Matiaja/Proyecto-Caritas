import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { PurchaseService } from '../../../services/purchase/purchase.service';
import { CommonModule } from '@angular/common';
import { UiTableComponent } from "../../../shared/components/ui-table/ui-table.component";
import { PdfService, PdfGenerationRequest } from '../../../services/pdf/pdf.service';

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

  constructor(
    private route: ActivatedRoute,
    private purchaseService: PurchaseService,
    private pdfService: PdfService
  ) {}

  ngOnInit(): void {
    this.purchaseId = Number(this.route.snapshot.paramMap.get('id'));
    this.purchaseService.getById(this.purchaseId).subscribe(p => {
      this.purchase = p;
      this.items = p.items;
    });
  }

  toggleDetails(distId: number): void {
    this.expandedDistId = this.expandedDistId === distId ? null : distId;
  }

  printPurchase(): void {
    if (!this.purchase) { return; }

    const req: PdfGenerationRequest = {
      title: 'Abordaje comunitario proyecto pnud.Arg/20/004-secretaria nacional de la niñez, adolescencia, familia',
      rightNotes: ['MINISTERIO CAPITAL HUMANO', 'CARACTER DE DECLARACION JURADA'],
      subtitle: `Compra #${this.purchase.id}`,
      generatedDate: new Date(),
      sections: [
        {
          title: 'Datos de la Compra',
          keyValuePairs: [
            { key: 'Fecha', value: this.purchase.purchaseDate ? new Date(this.purchase.purchaseDate).toLocaleDateString('es-AR') : '-' },
            { key: 'Origen', value: this.purchase.centerName || '-' },
            { key: 'Tipo', value: this.purchase.type || '-' }
          ]
        }
      ],
      tableData: {
        title: 'Productos',
        headers: ['Producto', 'Cantidad', 'Restante'],
        rows: (this.items || []).map(i => [
          i.productName || '-',
          (i.quantity ?? '-').toString(),
          (i.remainingQuantity ?? '-').toString()
        ])
      },
      footer: 'Generado por Sistema Cáritas',
      orientation: 'landscape',
      signatureAreas: [
        'Firma y Aclaración del Responsable de Entrega',
        'Firma y Aclaración del Receptor'
      ]
    };

    this.pdfService.generatePdf(req).subscribe(blob => this.pdfService.openPdfInNewTab(blob));
  }

  printDistribution(dist: any, $event?: Event): void {
    console.log('Printing distribution:', dist);
    $event?.stopPropagation();
    if (!this.purchase || !dist) { return; }

    const destino = dist.centerId !== 0 ? (dist.centerName || '-') : (dist.personName || '-');

    const req: PdfGenerationRequest = {
      title: 'Abordaje comunitario proyecto pnud.Arg/20/004-secretaria nacional de la niñez, adolescencia, familia',
      rightNotes: ['MINISTERIO CAPITAL HUMANO', 'CARACTER DE DECLARACION JURADA'],
      subtitle: `Compra #${this.purchase.id} - Entrega #${dist.id}`,
      generatedDate: new Date(),
      sections: [
        {
          title: 'Datos de la Compra',
          keyValuePairs: [
            { key: 'Fecha de Compra', value: this.purchase.purchaseDate ? new Date(this.purchase.purchaseDate).toLocaleDateString('es-AR') : '-' },
            { key: 'Origen', value: this.purchase.centerName || '-' },
            { key: 'Tipo', value: this.purchase.type || '-' }
          ],
          sideBySideWithNext: true
        },
        {
          title: 'Datos de la Entrega',
          keyValuePairs: [
            { key: 'Fecha de Entrega', value: dist.deliveryDate ? new Date(dist.deliveryDate).toLocaleDateString('es-AR') : '-' },
            { key: 'Destino', value: destino },
            { key: 'Estado', value: dist.status || '-' }
          ]
        }
      ],
      tableData: {
        title: 'Listado de Receptores',
        headers: [
          'Numero',
          'Nombre y apellido',
          'DNI',
          'Direccion/Ciudad',
          'Integrante de familia',
          'Firma'
        ],
        rows: [[
          '1',
          dist.personName || '-',
          dist.personDNI || '-',
          dist.personLocation || '-',
          dist.personMemberFamily || '-',
          '' // espacio para firma en la celda
        ]]
      },
      footer: 'Generado por Sistema Cáritas',
      orientation: 'landscape',
      signatureAreas: [
        'Firma y Aclaración del Responsable de Entrega',
        'Firma y Aclaración del Receptor'
      ]
    };

    this.pdfService.generatePdf(req).subscribe(blob => this.pdfService.openPdfInNewTab(blob));
  }
}

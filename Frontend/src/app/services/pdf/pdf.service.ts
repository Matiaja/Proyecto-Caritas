import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PdfGenerationRequest {
  title: string;
  subtitle?: string;
  generatedDate?: Date;
  sections?: PdfSection[];
  tableData?: PdfTableData;
  logoBase64?: string;
  footer?: string;
  orientation?: 'portrait' | 'landscape';
  signatureAreas?: string[];
  rightNotes?: string[];
}

export interface PdfSection {
  title: string;
  content?: string;
  keyValuePairs?: { key: string; value: string }[];
}

export interface PdfTableData {
  headers: string[];
  rows: string[][];
  title?: string;
}

@Injectable({
  providedIn: 'root'
})
export class PdfService {
  private baseUrl = environment.baseUrl + 'pdf';

  constructor(private http: HttpClient) {}

  generatePdf(request: PdfGenerationRequest): Observable<Blob> {
    return this.http.post(`${this.baseUrl}/generate`, request, {
      responseType: 'blob',
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })
    });
  }

  generateStockDetailPdf(productId: number, centerId: number): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/stock-detail/${productId}/${centerId}`, {
      responseType: 'blob'
    });
  }

  generateRequestDetailPdf(requestId: number): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/request-detail/${requestId}`, {
      responseType: 'blob'
    });
  }

  generateRequestDetailPdfWithData(requestData: any): Observable<Blob> {
    const pdfRequest: PdfGenerationRequest = {
      title: 'Detalle de Solicitud',
      subtitle: `Solicitud Nro. ${requestData.id}`,
      sections: [
        {
          title: 'Información de la Solicitud',
          keyValuePairs: [
            { key: 'Centro Solicitante', value: requestData.requestingCenter?.name || 'No disponible' },
            { key: 'Fecha de Solicitud', value: requestData.requestDate ? new Date(requestData.requestDate).toLocaleDateString('es-AR') : 'No disponible' },
            { key: 'Nivel de Urgencia', value: requestData.urgencyLevel || 'No especificado' },
            { key: 'Estado', value: requestData.status || 'No disponible' }
          ]
        }
      ],
      tableData: requestData.orderLines && requestData.orderLines.length > 0 ? {
        title: 'Productos Solicitados',
        headers: ['Código', 'Producto', 'Cantidad', 'Descripción', 'Asignado'],
        rows: requestData.orderLines.map((line: any) => [
          line.id?.toString() || '-',
          line.productName || 'Producto no encontrado',
          line.quantity?.toString() || '-',
          line.description || '-',
          line.isAssigned || 'No'
        ])
      } : undefined,
      footer: `Generado el ${new Date().toLocaleDateString('es-AR')} por Sistema Cáritas`
    };

    return this.generatePdf(pdfRequest);
  }

  generateStockDetailPdfWithData(productData: any, centerData: any, stockData: any[]): Observable<Blob> {
    const pdfRequest: PdfGenerationRequest = {
      title: `Detalle de Stock - ${productData.name}`,
      subtitle: `${productData.name} - ${centerData.name || 'Centro'}`,
      sections: [
        {
          title: 'Información del Producto',
          keyValuePairs: [
            { key: 'Producto', value: productData.name || 'No disponible' },
            { key: 'Código', value: productData.code || 'No disponible' },
            { key: 'Centro', value: centerData.name || 'No disponible' }
          ]
        }
      ],
      tableData: {
        title: 'Movimientos de Stock',
        headers: ['Tipo', 'Cantidad', 'Descripción', 'Origen', 'Fecha', 'Fecha Exp.', 'Peso'],
        rows: stockData.map(item => [
          item.type || '-',
          item.quantity?.toString() || '-',
          item.description || '-',
          item.origin || '-',
          item.date ? new Date(item.date).toLocaleDateString('es-AR') : '-',
          item.expirationDate ? new Date(item.expirationDate).toLocaleDateString('es-AR') : '-',
          (item.weight && item.weight > 0) ? item.weight.toString() : '-'
        ])
      },
      footer: `Generado el ${new Date().toLocaleDateString('es-AR')} por Sistema Cáritas`
    };

    return this.generatePdf(pdfRequest);
  }

  downloadPdf(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.click();
    window.URL.revokeObjectURL(url);
  }

  openPdfInNewTab(blob: Blob): void {
    const url = window.URL.createObjectURL(blob);
    window.open(url, '_blank');
  }
}
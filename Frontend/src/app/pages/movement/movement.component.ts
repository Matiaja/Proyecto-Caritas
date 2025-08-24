import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { Movement } from '../../models/movement.model';
import { MovementService } from '../../services/movement/movement.service';
import { MatFormField, MatFormFieldModule, MatLabel } from "@angular/material/form-field";
import {MatDatepickerModule} from '@angular/material/datepicker';
import {MatInputModule} from '@angular/material/input';
import { MAT_DATE_LOCALE, MatNativeDateModule, MatOption } from "@angular/material/core";
import { MatSelect, MatSelectModule } from "@angular/material/select";
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { CommonModule } from '@angular/common';
import {provideNativeDateAdapter} from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { UiTableComponent } from "../../shared/components/ui-table/ui-table.component";
import { ResponsiveService } from '../../services/responsive/responsive.service';
import { Router } from '@angular/router';
import { CenterService } from '../../services/center/center.service';
import { CenterModel } from '../../models/center.model';
import { PdfService, PdfGenerationRequest } from '../../services/pdf/pdf.service';

@Component({
  selector: 'app-movement',
  standalone: true,
  providers: [{provide: MAT_DATE_LOCALE, useValue: 'es-AR'},
provideNativeDateAdapter()],
  imports: [MatFormField,
    MatLabel,
    MatOption,
    MatSelect,
    MatDatepickerModule,
    MatNativeDateModule,
    MatFormFieldModule,
    MatInputModule,
    FormsModule,
    MatTableModule,
    MatSelectModule,
    MatButtonModule,
    CommonModule, UiTableComponent],
  templateUrl: './movement.component.html',
  styleUrl: './movement.component.css',
  encapsulation: ViewEncapsulation.None
})
export class MovementComponent implements OnInit {
  // Datos de la tabla
  movements: Movement[] = [];
  displayedColumns = ['fromCenter', 'toCenter', 'productName', 'quantity', 'status', 'updatedDate'];
  columnHeaders = {
    fromCenter: 'Desde',
    toCenter: 'Hacia',
    productName: 'Producto',
    quantity: 'Cantidad',
    status: 'Estado',
    updatedDate: 'Fecha'
  };
  title = 'Movimientos';
  mobileColumns = ['fromCenter', 'toCenter', 'productName', 'quantity', 'updatedDate'];
  mobileHeaders = {
    fromCenter: 'Desde',
    toCenter: 'Hacia',
    productName: 'Producto',
    quantity: 'Cant.',
    updatedDate: 'Fecha'
  };

  // Filtros
  filtersVisible = false;
  dateFrom: Date | null = null;
  dateTo: Date | null = null;
  status: string | null = null;
  productName: string | null = null;
  centerId: number | null = null;
  typeCenter: string | null = null;

  movementType = 'donations';
  showDetail = true;

  centers: CenterModel[] = [];

  isMobile = false;
  isPrinting = false;

  constructor(
    private movementService: MovementService,
    private centerService: CenterService,
    private responsiveService: ResponsiveService,
    private router: Router,
    private pdfService: PdfService
  ) {
      this.responsiveService.isMobile$.subscribe((isMobile) => {
        this.isMobile = isMobile;
      });
  }

  ngOnInit(): void {
    // Si ya hay un valor guardado, usarlo
    const savedType = localStorage.getItem('movementType');
    if (savedType) {
      this.movementType = savedType;
    }
    this.centerService.getCenters().subscribe((centers) => {
        this.centers = centers;
      });
    this.loadMovements();
  }

  private getFiltersForApi() {
    return {
      dateFrom: this.dateFrom ? this.dateFrom.toISOString() : undefined,
      dateTo: this.dateTo
        ? new Date(
            this.dateTo.getFullYear(),
            this.dateTo.getMonth(),
            this.dateTo.getDate(),
            23, 59, 59
          ).toISOString()
        : undefined,
      status: this.status || undefined,
      productName: this.productName || undefined,
      centerId: this.centerId || undefined,
      typeCenter: this.typeCenter || undefined
    };
  }

  loadMovements() {
    const filters = this.getFiltersForApi();

    // Guardar selección actual
    localStorage.setItem('movementType', this.movementType);

    if (this.movementType === 'donations') {
      this.showDetail = true;
      this.movementService.getDonationMovements(filters).subscribe(this.handleResponse, this.handleError);
    } else if (this.movementType === 'distributions') {
      this.showDetail = false;
      this.movementService.getDistributionMovements(filters).subscribe(this.handleResponse, this.handleError);
    } else {
      // De almacén directo (endpoint futuro)
      // this.movementService.getDirectMovements(filters).subscribe(this.handleResponse, this.handleError);
    }
  }

  handleResponse = (data: any[]) => {
    this.movements = data.map(movement => ({
      ...movement,
      updatedDate: new Date(movement.updatedDate).toLocaleDateString('es-ES', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
      }) + ' ' + new Date(movement.updatedDate).toLocaleTimeString('es-ES', {
        hour: '2-digit',
        minute: '2-digit',
      })
    }));
  }

  handleError = (err: any) => {
    console.error('Error al obtener movimientos:', err);
  }

  // Agregar método para determinar el color según el estado
  getStatusColor(status: string): string {
    switch(status) {
      case 'Recibida':
        return 'bg-success-light'; // Verde claro
      case 'Rechazada':
        return 'bg-danger-light'; // Rojo claro
      default:
        return 'bg-light'; // Gris claro (para otros estados)
    }
  }

  getRowClass = (movement: Movement) => {
    return this.isMobile ? this.getStatusColor(movement.status) : '';
  }

  toggleFilters() {
    this.filtersVisible = !this.filtersVisible;
  }

  applyFilters(): void {
    this.loadMovements();
  }

  clearFilters(): void {
    this.dateFrom = null;
    this.dateTo = null;
    this.status = null;
    this.productName = null;
    this.centerId = null;
    this.typeCenter = null;
    this.loadMovements();
  }

  onSelectMovement(m: Movement) {
    if(this.movementType === 'donations') {
      this.router.navigate(['/movements', m.donationRequestId]);
    }
  }

  // Generar PDF con TODOS los datos (consulta directa a la API con filtros actuales)
  printMovements(): void {
    this.isPrinting = true;

    const filters = this.getFiltersForApi();
    const obs = this.movementType === 'distributions'
      ? this.movementService.getDistributionMovements(filters)
      : this.movementService.getDonationMovements(filters);

    obs.subscribe({
      next: (all) => {
        const headers = ['Desde', 'Hacia', 'Producto', 'Cantidad', 'Estado', 'Fecha'];
        const rows = (all || []).map((m: any) => {
          const dateRaw = m.updatedDate ?? m.lastStatusChangeDate ?? m.assignmentDate;
          const fecha = dateRaw ? new Date(dateRaw).toLocaleString('es-AR') : '-';
          return [
            m.fromCenter ?? '-',
            m.toCenter ?? '-',
            m.productName ?? '-',
            (m.quantity != null ? String(m.quantity) : '-'),
            m.status ?? '-',
            fecha
          ];
        });

        const filtersKeyValue = this.buildFiltersSummaryKeyValueForMovements();

        const req: PdfGenerationRequest = {
          title: this.movementType === 'distributions'
            ? 'Movimientos - Compras / Bolsones'
            : 'Movimientos - Donaciones',
          subtitle: 'Listado de movimientos',
          orientation: 'landscape',
          sections: [
            {
              title: 'Filtros aplicados',
              keyValuePairs: filtersKeyValue
            }
          ],
          rightNotes: [
            `Total registros: ${rows.length}`
          ],
          tableData: [{
            title: 'Movimientos',
            headers,
            rows
          }],
          footer: `Generado el ${new Date().toLocaleString('es-AR')} por Sistema Cáritas`
        };

        this.pdfService.generatePdf(req).subscribe({
          next: (blob) => {
            this.pdfService.openPdfInNewTab(blob);
            this.isPrinting = false;
          },
          error: (err) => {
            console.error('Error generando PDF', err);
            this.isPrinting = false;
          }
        });
      },
      error: (err) => {
        console.error('Error cargando datos para impresión', err);
        this.isPrinting = false;
      }
    });
  }

  // Resumen de filtros similar a Home
  private buildFiltersSummaryKeyValueForMovements(): { key: string; value: string }[] {
    const result: { key: string; value: string }[] = [];

    result.push({
      key: 'Tipo de movimiento',
      value: this.movementType === 'distributions' ? 'Compras / Bolsones' : 'Donaciones'
    });

    const from = this.dateFrom ? new Date(this.dateFrom).toLocaleDateString('es-AR') : null;
    const to = this.dateTo ? new Date(this.dateTo).toLocaleDateString('es-AR') : null;

    let periodo = 'Histórico';
    if (from && !to) periodo = `a partir del ${from}`;
    if (!from && to) periodo = `hasta el ${to}`;
    if (from && to) periodo = `entre el ${from} y el ${to}`;

    result.push({ key: 'Período', value: periodo });
    result.push({ key: 'Estado', value: this.status && this.status !== '' ? this.status : 'Todos' });
    result.push({ key: 'Producto', value: this.productName && this.productName !== '' ? this.productName : 'Todos' });

    // Centro
    let centerValue = 'Todos';
    if (this.centerId != null) {
      const c = this.centers.find(x => x.id === this.centerId);
      centerValue = c ? c.name : `Centro #${this.centerId}`;
    }
    result.push({ key: 'Centro', value: centerValue });

    // Tipo de centro (si aplica)
    if (this.centerId != null) {
      const tc = this.typeCenter === 'from' ? 'Emisor'
        : this.typeCenter === 'to' ? 'Receptor'
        : 'Todos';
      result.push({ key: 'Tipo de centro', value: tc });
    }

    return result;
  }
}

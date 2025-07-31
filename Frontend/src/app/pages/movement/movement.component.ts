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
  displayedColumns = ['fromCenter', 'toCenter', 'productName', 'quantity', 'status', 'assignmentDate'];
  columnHeaders = {
    fromCenter: 'Desde',
    toCenter: 'Hacia',
    productName: 'Producto',
    quantity: 'Cantidad',
    status: 'Estado',
    assignmentDate: 'Fecha'
  };
  title = 'Movimientos entre centros';
  mobileColumns = ['fromCenter', 'toCenter', 'productName', 'quantity', 'assignmentDate'];
  mobileHeaders = {
    fromCenter: 'Desde',
    toCenter: 'Hacia',
    productName: 'Producto',
    quantity: 'Cant.',
    assignmentDate: 'Fecha'
  };

  // Filtros
  filtersVisible = false;
  dateFrom: Date | null = null;
  dateTo: Date | null = null;
  status: string | null = null;
  productName: string | null = null;

  constructor(private movementService: MovementService) {}

  ngOnInit(): void {
    this.loadMovements();
  }

  loadMovements() {

    const filters = {
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
      productName: this.productName || undefined
    };

    this.movementService.getMovements(filters).subscribe({
      next: (data) => {
        this.movements = data.map(movement => ({
          ...movement,
          assignmentDate: new Date(movement.assignmentDate).toLocaleDateString('es-ES', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
          }) + ' ' + new Date(movement.assignmentDate).toLocaleTimeString('es-ES', {
            hour: '2-digit',
            minute: '2-digit',
          })
        }));
      },
      error: (err) => {
        console.error('Error al obtener movimientos:', err);
      }
    });
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
    this.loadMovements();
  }
}

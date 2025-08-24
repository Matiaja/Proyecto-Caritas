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
import { GlobalStateService } from '../../services/global/global-state.service';
import { AuthService } from '../../auth/auth.service';

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
  allMovements: Movement[] = [];
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
  userCenterId = 0;
  isAdmin = false;

  constructor(
    private movementService: MovementService,
    private centerService: CenterService,
    private responsiveService: ResponsiveService,
    private globalStateService: GlobalStateService,
    private authService: AuthService,
    private router: Router
  ) {
      this.responsiveService.isMobile$.subscribe((isMobile) => {
        this.isMobile = isMobile;
      });
      this.userCenterId = this.globalStateService.getCurrentCenterId();
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
    this.checkAdminRole();
    this.loadMovements();
  }

  checkAdminRole(): void {
    this.isAdmin = this.authService.isAdmin();
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
      // productName removed from server filter
      centerId: this.centerId || undefined,
      typeCenter: this.typeCenter || undefined
    };

    // Guardar selección actual
    localStorage.setItem('movementType', this.movementType);

    const handleResponseLocal = (data: any[]) => {
      this.allMovements = data.map(movement => ({
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
      this.applyProductFilter();
    };

    if (this.movementType === 'donations') {
      this.showDetail = true;
      this.movementService.getDonationMovements(filters).subscribe({next: handleResponseLocal, error: this.handleError});
    } else if (this.movementType === 'distributions') {
      this.showDetail = false;
      this.movementService.getDistributionMovements(filters).subscribe({next: handleResponseLocal, error: this.handleError});
    } else if (this.movementType === 'storage') {
      this.showDetail = false;
      this.movementService.getStorageMovements(filters).subscribe({next: handleResponseLocal, error: this.handleError});
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
    if(this.movementType == 'storage')
    {
      if(this.userCenterId !== 0) {
        this.centerId = this.userCenterId;
      } else {
        this.centerId = null;
      }
    }
    console.log(this.centerId);
    this.loadMovements();
  }

  applyProductFilter(): void {
    if (!this.productName || this.productName.trim() === '') {
      this.movements = [...this.allMovements];
    } else {
      const filterValue = this.productName.trim().toLowerCase();
      this.movements = this.allMovements.filter(mov =>
        mov.productName && mov.productName.toLowerCase().includes(filterValue)
      );
    }
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
}

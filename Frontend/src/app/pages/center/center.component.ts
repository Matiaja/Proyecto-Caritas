import { Component, OnInit } from '@angular/core';
import { CenterModel } from '../../models/center.model';
import { CenterService } from '../../services/center/center.service';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { UiTableComponent } from '../../shared/components/ui-table/ui-table.component';
import { Router } from '@angular/router';
import { ConfirmModalService } from '../../services/confirmModal/confirm-modal.service';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../../auth/auth.service';

@Component({
  selector: 'app-center',
  standalone: true,
  imports: [CommonModule, UiTableComponent],
  templateUrl: './center.component.html',
  styleUrl: './center.component.css',
})
export class CenterComponent implements OnInit {
  title = 'Centros';

  sortBy = '';
  order = 'asc';
  sortOptions = [
    { key: 'name', label: 'Nombre' },
    { key: 'location', label: 'Ubicación' },
    { key: 'manager', label: 'Encargado' },
  ];
  canEdit: boolean = false;
  canDelete: boolean = false;
  canAdd: boolean = false;

  columnHeaders: Record<string, string> = {
    name: 'Nombre',
    location: 'Ubicación',
    manager: 'Encargado',
    phone: 'Teléfono',
    email: 'Correo Electrónico',
  };
  displayedColumns = ['name', 'location', 'manager', 'phone', 'email'];
  centers: CenterModel[] = [];

  constructor(
    private centerService: CenterService,
    private router: Router,
    private modalService: ConfirmModalService,
    private toastr: ToastrService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.checkRole();
    this.loadCenters();
  }
  
  checkRole() {
    const userRole = this.authService.getUserRole();
    this.canEdit = userRole === 'Admin';
    this.canDelete = userRole === 'Admin';
    this.canAdd = userRole === 'Admin';
  }

  loadCenters() {
    // Pass sortBy and order parameters to the service
    this.centerService.getFilteredCenters(this.sortBy, this.order).subscribe({
      next: (centers: CenterModel[]) => {
        this.centers = centers;
      },
      error: (err) => {
        // Using toastr for user-facing errors is better than console.log
        this.toastr.error('No se pudieron cargar los centros.', 'Error');
        console.log(err);
      },
    });
  }
  
  onFilterChange(filters: { sortBy?: string; order?: string }) {
    this.sortBy = filters.sortBy || '';
    this.order = filters.order || 'asc';
    this.loadCenters();
  }
  onAddCenter() {
    this.router.navigate(['/centers/add']);
  }

  onEditCenter(center: CenterModel) {
    this.router.navigate(['/centers/edit', center.id]);
  }

  async onDeleteCenter(center: any) {
    const confirmed = await this.modalService.confirm(
      'Eliminar Centro',
      '¿Estás seguro de que quieres eliminar este centro?'
    );

    if (confirmed) {
    this.centerService.deleteCenter(center.id).subscribe({
      next: (response: any) => {
        // --- THIS CODE RUNS ONLY ON SUCCESS (e.g., status 200 OK) ---
        // The success toastr is now INSIDE the success block.
        this.toastr.success(response.message || 'Centro eliminado correctamente', 'Éxito');
        
        // The list is updated only after a successful deletion.
        this.centers = this.centers.filter((p) => p.id !== center.id);
      },
      error: (err: HttpErrorResponse) => {
        // --- THIS CODE RUNS ONLY ON FAILURE (e.g., status 409, 500) ---
        // We show the specific error message from the API.
        const errorMessage = err.error?.message || 'Ocurrió un error al eliminar el centro.';
        this.toastr.error(errorMessage, 'Error');
      }
    });
    }
  }

  onSelectCenter(center: CenterModel) {
    this.router.navigate(['/center', center.id]);
  }
}

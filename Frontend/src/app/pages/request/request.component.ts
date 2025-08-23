import { Component, OnInit } from '@angular/core';
import { RequestModel } from '../../models/request.model';
import { RequestService } from '../../services/request/request.service';
import { CommonModule } from '@angular/common';
import { UiTableComponent } from '../../shared/components/ui-table/ui-table.component';
import { Router } from '@angular/router';
import { ResponsiveService } from '../../services/responsive/responsive.service';
import { ConfirmModalService } from '../../services/confirmModal/confirm-modal.service';
import { ToastrService } from 'ngx-toastr';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-request',
  standalone: true,
  imports: [CommonModule, UiTableComponent, FormsModule],
  templateUrl: './request.component.html',
  styleUrl: './request.component.css',
})
export class RequestComponent implements OnInit {
  title = 'Solicitudes';
  columnHeaders: Record<string, string> = {
    id: '#',
    requestDate: 'Fecha de solicitud',
    centerName: 'Centro/Parroquia',
    status: 'Estado',
    urgencyLevel: 'Urgencia',
  };
  displayedColumns = ['id','requestDate', 'centerName', 'status', 'urgencyLevel'];
  mobileHeaders: Record<string, string> = {
    id: '#',
    requestDate: 'Fecha',
    centerName: 'Centro',
    urgencyLevel: 'Urgencia',
  };
  mobileColumns = ['id','requestDate', 'centerName', 'urgencyLevel'];
  requests: RequestModel[] = [];

  // filters

  selectedStatus: string | null = null;
  selectedUrgency: string | null = null;
  sortBy = '';
  sortOptions = [
    { key: 'requestDate', label: 'Fecha' },
    { key: 'urgencyLevel', label: 'Urgencia' },
  ];
  order = 'asc';
  searchColumns = ['centerName', 'requestDate'];
  statusOptions = ['Pendiente', 'Finalizada'];
  urgencyOptions = ['Alto', 'Bajo'];

  page = 1;
  itemsPerPage = 10;
  totalItems = 0;

  isMobile = false;
  constructor(
    private reqService: RequestService,
    private router: Router,
    private modalService: ConfirmModalService,
    private toastr: ToastrService,
    private responsiveService: ResponsiveService
  ) {
    this.responsiveService.isMobile$.subscribe((isMobile) => {
      this.isMobile = isMobile;
    });
  }

  ngOnInit() {
    this.loadRequests();
  }

  onFilterChange(filters: { status?: string; urgencyLevel?: string; sortBy?: string; order?: string }) {
    this.selectedStatus = filters.status || null;
    this.selectedUrgency = filters.urgencyLevel || null;
    this.sortBy = filters.sortBy || '';
    this.order = filters.order || 'asc';
    this.loadRequests();
  }

  loadRequests() {
    this.reqService.getRequests().subscribe({
      next: (reqs: RequestModel[]) => {
        let filtered = reqs;

        // Filtro por estado
        if (this.selectedStatus) {
          filtered = filtered.filter((r) => r.status === this.selectedStatus);
        }

        // Filtro por urgencia
        if (this.selectedUrgency) {
          filtered = filtered.filter((r) => r.urgencyLevel === this.selectedUrgency);
        }

        // Ordenamiento
        if (this.sortBy) {
          filtered = filtered.sort((a: any, b: any) => {
            const aValue = a[this.sortBy];
            const bValue = b[this.sortBy];

            if (aValue < bValue) return this.order === 'asc' ? -1 : 1;
            if (aValue > bValue) return this.order === 'asc' ? 1 : -1;
            return 0;
          });
        }

        this.requests = filtered.map((req) => ({
          ...req,
          requestDate: new Date(req.requestDate).toLocaleDateString('es-ES', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
          }),
          centerName: req.requestingCenter?.name,
        }));
      },
      error: (err) => {
        console.error(err);
      },
    });
  }

  onPageChange(newPage: number) {
    this.page = newPage;
    this.loadRequests();
  }

  onAddRequest() {
    this.router.navigate(['/requests/add']);
  }

  async close(row: any) {
    const confirmed = await this.modalService.confirm(
      'Cerrar solicitud',
      'Esta solicitud se cerrará manualmente y no podrá recibir más donaciones. ¿Confirmás su finalización?'
    );

    if (confirmed) {
      this.reqService.closeRequest(row.id).subscribe({
        next: () => {
          this.toastr.success('Solicitud cerrada correctamente');
          // Reload requests
          this.loadRequests();
        },
        error: (err) => {
          console.error(err);
          this.toastr.error(err.error || 'Ocurrió un error al cerrar la solicitud');
        }
      });
    }
  }

  onEditRequest(req: RequestModel) {
    console.log('Edit request', req);
  }
  onDeleteRequest(req: RequestModel) {
    console.log('Delete request', req);
  }
  onSelectRequest(req: RequestModel) {
    this.router.navigate(['/requests', req.id]);
  }
}

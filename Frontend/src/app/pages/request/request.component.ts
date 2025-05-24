import { Component, OnInit } from '@angular/core';
import { RequestModel } from '../../models/request.model';
import { RequestService } from '../../services/request/request.service';
import { CommonModule } from '@angular/common';
import { UiTableComponent } from '../../shared/components/ui-table/ui-table.component';
import { Router } from '@angular/router';
import { BreadcrumbComponent } from '../../shared/components/breadcrumbs/breadcrumbs.component';

@Component({
  selector: 'app-request',
  standalone: true,
  imports: [CommonModule, UiTableComponent, BreadcrumbComponent],
  templateUrl: './request.component.html',
  styleUrl: './request.component.css',
})
export class RequestComponent implements OnInit {
  title = 'Solicitudes';
  columnHeaders: Record<string, string> = {
    requestDate: 'Fecha de solicitud',
    centerName: 'Centro/Parroquia',
    status: 'Estado',
    urgencyLevel: 'Urgencia',
  };
  displayedColumns = ['requestDate', 'centerName', 'status', 'urgencyLevel'];
  mobileHeaders: Record<string, string> = {
    requestDate: 'Fecha de solicitud',
    centerName: 'Centro/Parroquia',
    urgencyLevel: 'Urgencia',
  };
  mobileColumns = ['requestDate', 'centerName', 'urgencyLevel'];
  requests: RequestModel[] = [];

  // filters
  
  selectedStatus: string | null = null;
  sortBy = '';
  sortOptions = [
    { key: 'requestDate', label: 'Fecha' },
    { key: 'urgencyLevel', label: 'Urgencia' },
  ];
  order = 'asc';
  searchColumns = ['centerName', 'requestDate'];
  statusOptions = ['Pendiente', 'Finalizada'];

  page = 1;
  itemsPerPage = 10;
  totalItems = 0;

  constructor(
    private reqService: RequestService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadRequests();
  }

  onFilterChange(filters: { status?: string; sortBy?: string; order?: string }) {
    this.selectedStatus = filters.status || null;
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
        
        console.log('Filtered', filtered);
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

import { Component, OnInit } from '@angular/core';
import { RequestModel } from '../../models/request.model';
import { RequestService } from '../../services/request/request.service';
import { CommonModule } from '@angular/common';
import { UiTableComponent } from "../../shared/components/ui-table/ui-table.component";
import { Router } from '@angular/router';
import { BreadcrumbComponent } from '../../shared/components/breadcrumbs/breadcrumbs.component';

@Component({
  selector: 'app-request',
  standalone: true,
  imports: [CommonModule, UiTableComponent, BreadcrumbComponent],
  templateUrl: './request.component.html',
  styleUrl: './request.component.css'
})
export class RequestComponent implements OnInit{
    title = 'Solicitudes';
    columnHeaders: { [key: string]: string } = {
      centerName: 'Centro/Parroquia',
      requestDate: 'Fecha de solicitud',
      urgencyLevel: 'Urgencia',
    };
  displayedColumns = ['centerName', 'requestDate', 'urgencyLevel'];
  requests: RequestModel[] = [];

  constructor(
    private reqService: RequestService, 
    private router: Router,
  ) { }
  
  ngOnInit() {
    this.reqService.getRequests().subscribe({
      next: (reqs: RequestModel[]) => {
        this.requests = reqs.map(req => ({
          ...req, // Copia el resto de las propiedades
          requestDate: new Date(req.requestDate).toLocaleDateString('es-ES', { day: '2-digit', month: '2-digit', year: 'numeric' }),
          centerName: req.requestingCenter?.name
        }));
        console.log(this.requests);
      },
      error: (err) => {
        console.log(err);
      }
    });
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

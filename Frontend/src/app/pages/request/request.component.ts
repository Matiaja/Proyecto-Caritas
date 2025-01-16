import { Component, OnInit } from '@angular/core';
import { Request } from '../../models/request';
import { RequestService } from '../../services/request.service';
import { CommonModule } from '@angular/common';
import { UiTableComponent } from "../../shared/components/ui-table/ui-table.component";

@Component({
  selector: 'app-request',
  standalone: true,
  imports: [CommonModule, UiTableComponent],
  templateUrl: './request.component.html',
  styleUrl: './request.component.css'
})
export class RequestComponent implements OnInit{
  displayedColumns = ['id', 'centerName', 'requestDate', 'urgencyLevel'];
  requests: Request[] = [];

  constructor(private reqService: RequestService) { }
  
  ngOnInit() {
    this.reqService.getRequests().subscribe({
      next: (reqs: Request[]) => {
        this.requests = reqs.map(req => ({
          ...req, // Copia el resto de las propiedades
          requestDate: new Date(req.requestDate).toLocaleDateString('es-ES', { day: '2-digit', month: '2-digit', year: 'numeric' }),
          centerName: req.requestingCenter.name
        }));
        console.log(this.requests);
      },
      error: (err) => {
        console.log(err);
      }
    });
  } 
  

}

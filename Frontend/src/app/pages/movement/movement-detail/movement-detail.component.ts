import { Component, OnInit } from '@angular/core';
import { DonationRequest } from '../../../models/donationRequest.model';
import { DonationRequestService } from '../../../services/donationRequest/donation-request.service';
import { ActivatedRoute } from '@angular/router';
import { UiTableComponent } from "../../../shared/components/ui-table/ui-table.component";

@Component({
  selector: 'app-movement-detail',
  standalone: true,
  imports: [UiTableComponent],
  templateUrl: './movement-detail.component.html',
  styleUrl: './movement-detail.component.css'
})
export class MovementDetailComponent implements OnInit {
  donationRequestId = 0;
  // Datos de la tabla
  donation: DonationRequest | null = null;
  statusHistory: {
    status: string;
    changeDate: Date | string;
  }[] = [];
  displayedColumns = ['changeDate', 'status'];
  columnHeaders = {
    changeDate: 'Fecha',
    status: 'Estado'
  };
  title = 'Historial de estados';
  mobileColumns = ['changeDate', 'status'];
  mobileHeaders = {
    changeDate: 'Fecha',
    status: 'Estado'
  };

  constructor(
    private route: ActivatedRoute, 
    private donationRequestService: DonationRequestService
  ) {}

  ngOnInit(): void {
    this.loadMovement();
  }

  loadMovement() {
    this.route.params.subscribe(params => {
      this.donationRequestId = +params['id'];
        this.donationRequestService.getDonationRequestById(this.donationRequestId).subscribe({
          next: (data) => {
            this.donation = data;
            this.statusHistory = data.statusHistory ? 
              data.statusHistory.map(item => ({
                ...item,
                changeDate: new Date(item.changeDate).toLocaleDateString('es-ES', {
                  day: '2-digit',
                  month: '2-digit',
                  year: 'numeric',
                }) + ' ' + new Date(item.changeDate).toLocaleTimeString('es-ES', {
                  hour: '2-digit',
                  minute: '2-digit',
                })
              })) : [];
          },
          error: (err) => {
            console.error('Error al obtener movimientos:', err);
          }
        });
    });
  }

  goBack() {
    window.history.back();
  }
}

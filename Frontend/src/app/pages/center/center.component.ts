import { Component, OnInit } from '@angular/core';
import { CenterModel } from '../../models/center.model';
import { CenterService } from '../../services/center/center.service';
import { CommonModule } from '@angular/common';
import { UiTableComponent } from '../../shared/components/ui-table/ui-table.component';
import { Router } from '@angular/router';
import { ConfirmModalService } from '../../services/confirmModal/confirm-modal.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-center',
  standalone: true,
  imports: [CommonModule, UiTableComponent],
  templateUrl: './center.component.html',
  styleUrl: './center.component.css',
})
export class CenterComponent implements OnInit {
  title = 'Centros';
  columnHeaders: Record<string, string> = {
    name: 'Nombre',
    location: 'Ubicación',
    manager: 'Encargado',
    phone: 'Teléfono',
    email: 'Correo Electrónico',
  };
  displayedColumns = ['name', 'location', 'manager', 'phone', 'email'];
  centers: CenterModel[] = [];
  //modalService: any;

  constructor(
    private centerService: CenterService,
    private router: Router,
    private modalService: ConfirmModalService,
    private toastr: ToastrService
  ) {}

  ngOnInit() {
    this.centerService.getCenters().subscribe({
      next: (centers: CenterModel[]) => {
        this.centers = centers;
      },
      error: (err) => {
        console.log(err);
      },
    });
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
      this.centerService.deleteCenter(center.id).subscribe(() => {
        this.centers = this.centers.filter((p) => p.id !== center.id);
      });
      this.toastr.success('Centro eliminado correctamente');
    }
  }

  onSelectCenter(center: CenterModel) {
    this.router.navigate(['/center', center.id]);
  }
}

import { Component } from '@angular/core';
import { Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { GenericFormComponent } from '../../../shared/components/generic-form/generic-form.component';
import { BreadcrumbComponent } from '../../../shared/components/breadcrumbs/breadcrumbs.component';
import { CenterService } from '../../../services/center/center.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-center-add',
  standalone: true,
  imports: [GenericFormComponent, BreadcrumbComponent],
  templateUrl: './center-add.component.html',
  styleUrl: './center-add.component.css',
})
export class CenterAddComponent {
  formConfig = {
    title: 'Agregar Centro',
    fields: [
      {
        name: 'name',
        label: 'Nombre del Centro',
        type: 'text',
        value: '',
        placeholder: 'Ingrese el nombre del centro',
        validators: [Validators.required, Validators.pattern(/\S/)],
        errorMessage: 'El nombre del centro es requerido',
      },
      {
        name: 'location',
        label: 'Ubicación',
        type: 'text',
        value: '',
        placeholder: 'Ingrese la ubicación',
        validators: [Validators.required, Validators.pattern(/\S/)],
        errorMessage: 'La ubicación es requerida',
      },
      {
        name: 'manager',
        label: 'Encargado',
        type: 'text',
        value: '',
        placeholder: 'Nombre del encargado',
        validators: [Validators.required, Validators.pattern(/\S/)],
        errorMessage: 'El nombre del encargado es requerido',
      },
      {
        name: 'capacityLimit',
        label: 'Capacidad Máxima',
        type: 'number',
        value: '',
        placeholder: 'Ingrese la capacidad máxima',
        validators: [Validators.required, Validators.min(1)],
        errorMessage: 'La capacidad máxima es requerida y debe ser mayor a 0',
      },
      {
        name: 'phone',
        label: 'Teléfono',
        type: 'text',
        value: '',
        placeholder: 'Ingrese el teléfono de contacto',
        validators: [Validators.required, Validators.pattern(/^[0-9]+$/)],
        errorMessage: 'El número de teléfono es requerido',
      },
      {
        name: 'email',
        label: 'Correo Electrónico',
        type: 'email',
        value: '',
        placeholder: 'Ingrese el correo electrónico (opcional)',
        validators: [Validators.email, Validators.pattern(/\S/)],
        errorMessage: '',
      },
    ],
  };

  constructor(
    private centerService: CenterService,
    private router: Router,
    private toastr: ToastrService
  ) {}

  onSubmit(formData: any): void {
    const payload = {
      name: formData.name,
      location: formData.location,
      manager: formData.manager,
      capacityLimit: formData.capacityLimit,
      phone: formData.phone,
      email: formData.email,
    };

    this.centerService.createCenter(payload).subscribe(() => {
      this.toastr.success('Centro creado correctamente', 'Exito');
      this.router.navigate(['/centers']);
    });
  }

  onCancel(): void {
    this.router.navigate(['/centers']);
  }
}

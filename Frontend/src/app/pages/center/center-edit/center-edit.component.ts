import { Component, OnInit } from '@angular/core';
import { GenericFormComponent } from '../../../shared/components/generic-form/generic-form.component';
import { FormBuilder, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { CenterService } from '../../../services/center/center.service';
import { FormGroup } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-edit-center',
  standalone: true,
  imports: [GenericFormComponent],
  templateUrl: './center-edit.component.html',
  styleUrl: './center-edit.component.css',
})
export class CenterEditComponent implements OnInit {
  centerData: any;
  centerId!: number;
  form!: FormGroup;

  formConfig = {
    title: 'Editar Centro',
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
        errorMessage: 'El teléfono es requerido y solo debe contener números.',
      },
      {
        name: 'email',
        label: 'Correo Electrónico',
        type: 'email',
        value: '',
        placeholder: 'Ingrese el correo electrónico (opcional)',
        validators: [Validators.email],
        errorMessage: 'Formato de correo inválido',
      },
    ],
  };

  constructor(
    private centerService: CenterService,
    private router: Router,
    private route: ActivatedRoute,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.centerId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadCenterData();
  }

  loadCenterData(): void {
    this.centerService.getCenter(this.centerId).subscribe((center) => {
      this.centerData = center;

      this.formConfig = {
        title: 'Editar Centro',
        fields: [
          {
            name: 'name',
            label: 'Nombre del Centro',
            type: 'text',
            value: this.centerData.name,
            placeholder: 'Ingrese el nombre del centro',
            validators: [Validators.required, Validators.pattern(/\S/)],
            errorMessage: 'El nombre del centro es requerido',
          },
          {
            name: 'location',
            label: 'Ubicación',
            type: 'text',
            value: this.centerData.location,
            placeholder: 'Ingrese la ubicación',
            validators: [Validators.required, Validators.pattern(/\S/)],
            errorMessage: 'La ubicación es requerida',
          },
          {
            name: 'manager',
            label: 'Encargado',
            type: 'text',
            value: this.centerData.manager,
            placeholder: 'Nombre del encargado',
            validators: [Validators.required, Validators.pattern(/\S/)],
            errorMessage: 'El nombre del encargado es requerido',
          },
          {
            name: 'capacityLimit',
            label: 'Capacidad Máxima',
            type: 'number',
            value: this.centerData.capacityLimit ?? '', // Evita asignar undefined
            placeholder: 'Ingrese la capacidad máxima',
            validators: [Validators.required, Validators.min(1)],
            errorMessage: 'La capacidad máxima es requerida y debe ser mayor a 0',
          },
          {
            name: 'phone',
            label: 'Teléfono',
            type: 'text',
            value: this.centerData.phone,
            placeholder: 'Ingrese el teléfono de contacto',
            validators: [Validators.required, Validators.pattern(/^[0-9]+$/)],
            errorMessage: 'El teléfono es requerido y solo debe contener números.',
          },
          {
            name: 'email',
            label: 'Correo Electrónico',
            type: 'email',
            value: this.centerData.email ?? '', // Evita asignar undefined
            placeholder: 'Ingrese el correo electrónico (opcional)',
            validators: [Validators.email],
            errorMessage: 'Formato de correo inválido',
          },
        ],
      };
    });
  }

  onSubmit(formData: any): void {
    this.centerService.updateCenter(this.centerId, formData).subscribe(() => {
      this.toastr.success('Centro actualizado correctamente', 'Exito');
      this.router.navigate(['/centers']);
    });
  }

  onCancel(): void {
    this.router.navigate(['/centers']);
  }
}

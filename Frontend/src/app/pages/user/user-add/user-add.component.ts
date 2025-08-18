import { Component, OnInit } from '@angular/core';
import { GenericFormComponent } from '../../../shared/components/generic-form/generic-form.component';
import { Router } from '@angular/router';
import { Validators } from '@angular/forms';
import { BreadcrumbComponent } from '../../../shared/components/breadcrumbs/breadcrumbs.component';
import { UserService } from '../../../services/user/user.service';
import { CenterService } from '../../../services/center/center.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-user-add',
  standalone: true,
  imports: [GenericFormComponent, BreadcrumbComponent],
  templateUrl: './user-add.component.html',
  styleUrl: './user-add.component.css',
})
export class UserAddComponent implements OnInit {
  formConfig = {
    title: 'Agregar Usuario',
    fields: [
      {
        name: 'userName',
        label: 'Nombre de usuario',
        type: 'text',
        value: '',
        placeholder: 'Ingrese el nombre de usuario',
        validators: [Validators.required],
        errorMessage: 'El nombre de usuario es requerido',
      },
      {
        name: 'email',
        label: 'Correo electrónico',
        type: 'email',
        value: '',
        placeholder: 'Ingrese el correo electrónico',
        validators: [Validators.required, Validators.email],
      },
      {
        name: 'password',
        label: 'Contraseña',
        type: 'password',
        value: '',
        placeholder: 'Ingrese la contraseña',
        validators: [
          Validators.required,
          Validators.minLength(8),
          Validators.pattern('.*\\d.*'),
        ],
      },
      {
        name: 'FirstName',
        label: 'Nombre',
        type: 'text',
        value: '',
        placeholder: 'Ingrese el nombre',
        validators: [Validators.required],
        errorMessage: 'El nombre es requerido',
      },
      {
        name: 'LastName',
        label: 'Apellido',
        type: 'text',
        value: '',
        placeholder: 'Ingrese el apellido',
        validators: [Validators.required],
        errorMessage: 'El apellido es requerido',
      },
      {
        name: 'phoneNumber',
        label: 'Teléfono',
        type: 'tel',
        value: '',
        placeholder: 'Ingrese el teléfono',
        validators: [Validators.required],
      },
      {
        name: 'role',
        label: 'Rol',
        type: 'select',
        value: '',
        placeholder: 'Seleccione un rol',
        validators: [Validators.required],
        errorMessage: 'El rol es requerido',
        options: [
          { value: 'Admin', label: 'Administrador' },
          { value: 'User', label: 'Usuario' },
        ],
      },
      {
        name: 'center',
        label: 'Centro',
        type: 'select',
        value: '',
        placeholder: 'Seleccione un centro',
        validators: [Validators.required],
        errorMessage: 'El centro es requerido',
        options: [],
      },
    ],
  };

  constructor(
    private userService: UserService,
    private router: Router,
    private centerService: CenterService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.loadCenters();
  }

  loadCenters(): void {
    this.centerService.getCenters().subscribe((centers) => {
      const centerField = this.formConfig.fields.find((field) => field.name === 'center');
      if (centerField) {
        centerField.options = centers.map((center) => ({
          value: String(center.id),
          label: center.name,
        }));
      }
    });
  }

  onSubmit(formData: any): void {
    const payload = {
      userName: formData.userName,
      email: formData.email,
      password: formData.password,
      FirstName: formData.FirstName,
      LastName: formData.LastName,
      phoneNumber: formData.phoneNumber,
      role: formData.role,
      centerId: formData.center,
    };

    this.userService.registerUser(payload).subscribe({
      next: () => {
        this.toastr.success('Usuario creado correctamente', 'Exito');
        this.router.navigate(['/users']);
      },
      error: (err) => {
        console.error('Error al registrar usuario:', err);

        if (err.error?.message) {
          // Caso: Backend devolvió { message: "..." }
          this.toastr.error(err.error.message, 'Error');
        } else if (Array.isArray(err.error)) {
          // Caso: Backend devolvió lista de errores (Identity)
          this.toastr.error(err.error.map((e: any) => e.description).join('<br>'), 'Error', {
            enableHtml: true,
          });
        } else {
          // Caso genérico
          this.toastr.error('Ocurrió un error inesperado. Inténtelo nuevamente.', 'Error');
        }
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/users']);
  }
}

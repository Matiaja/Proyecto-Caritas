import { CommonModule } from '@angular/common';
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
        type: 'text',
        value: '',
        placeholder: 'Ingrese la contraseña',
        validators: [Validators.required, Validators.minLength(8), Validators.pattern('.*\\d.*')],
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
        ] as { value: any; label: string }[],
      },
      {
        name: 'center',
        label: 'Centro',
        type: 'select',
        value: '',
        placeholder: 'Seleccione un centro',
        validators: [Validators.required],
        errorMessage: 'El centro es requerido',
        options: [] as { value: any; label: string }[],
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
          value: center.id,
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
    this.userService.registerUser(payload).subscribe(() => {
      this.toastr.success('Usuario creado correctamente', 'Exito');
      this.router.navigate(['/users']);
    });
  }

  onCancel(): void {
    this.router.navigate(['/users']);
  }
}

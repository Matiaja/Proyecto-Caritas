import { Component, OnInit } from '@angular/core';
import { GenericFormComponent } from '../../../shared/components/generic-form/generic-form.component';
import { FormBuilder, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { FormGroup } from '@angular/forms';
import { BreadcrumbComponent } from '../../../shared/components/breadcrumbs/breadcrumbs.component';
import { UserService } from '../../../services/user/user.service';
import { GlobalStateService } from '../../../services/global/global-state.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-user-edit',
  standalone: true,
  imports: [GenericFormComponent],
  templateUrl: './user-edit.component.html',
  styleUrl: './user-edit.component.css'
})
export class UserEditComponent implements OnInit{
  userData: any;
  userId: string;
  form!: FormGroup;

  formConfig = {
    title : 'Editar Usuario',
    fields: [
      {
        name: 'email',
        label: 'Correo electrónico',
        type: 'email',
        value: '',
        placeholder: 'Ingrese el correo electrónico',
        validators: [Validators.required, Validators.email],
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
      },
      {
        name: 'phoneNumber',
        label: 'Teléfono',
        type: 'tel',
        value: '',
        placeholder: 'Ingrese el teléfono',
        validators: [Validators.required],
      }
    ],
  };

  constructor(
    private userService: UserService, 
    private globalStateService: GlobalStateService,
    private router: Router, 
    private route: ActivatedRoute, 
    private fb: FormBuilder,
    private toastr: ToastrService
    ) 
    {
      this.userId = '';
    }

  ngOnInit(): void {
    const userId = this.globalStateService.getCurrentUserId();
    this.userId = userId || '';
    console.log('User ID:', this.userId);
    this.loadUserData();
  }

  loadUserData(): void {
    this.userService.getUserById(this.userId).subscribe((user) => {
      this.userData = user;
      console.log(this.userData);
      this.formConfig = {
        title: 'Editar Usuario',
        fields: [
          {
            name: 'email',
            label: 'Correo electrónico',
            type: 'email',
            value: this.userData.email,
            placeholder: 'Ingrese el correo electrónico',
            validators: [Validators.required, Validators.email],
          },
          {
            name: 'FirstName',
            label: 'Nombre',
            type: 'text',
            value: this.userData.firstName,
            placeholder: 'Ingrese el nombre',
            validators: [Validators.required],
            errorMessage: 'El nombre es requerido',
          },
          {
            name: 'LastName',
            label: 'Apellido',
            type: 'text',
            value: this.userData.lastName,
            placeholder: 'Ingrese el apellido',
            validators: [Validators.required],
          },
          {
            name: 'phoneNumber',
            label: 'Teléfono',
            type: 'tel',
            value: this.userData.phoneNumber,
            placeholder: 'Ingrese el teléfono',
            validators: [Validators.required],
          }
        ],
      };
    });
  }

  onSubmit(formData: any): void {
    const user = {
      Id : this.userId,
      UserName: this.userData.userName,
      Email: formData.email,
      FirstName: formData.FirstName,
      LastName: formData.LastName,
      PhoneNumber: formData.phoneNumber,
      CenterId: this.userData.centerId,
      Role: this.userData.role,
    };
    console.log('User data:', user);
    this.userService.updateUser(user, this.userId).subscribe(
      (response) => {
        this.toastr.success('Usuario actualizado exitosamente', 'Exito');
        console.log('User updated successfully:', response);
        this.router.navigate(['/users/edit']);
      },
      (error) => {
        this.toastr.error('Error al actualizar el usuario', 'Error');
        console.error('Error updating user:', error);
      }
    );
  }

  onCancel(): void {
    this.router.navigate(['/home']);
  }

}

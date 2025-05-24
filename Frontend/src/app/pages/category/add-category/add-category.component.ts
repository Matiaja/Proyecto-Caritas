import { Component } from '@angular/core';
import { GenericFormComponent } from '../../../shared/components/generic-form/generic-form.component';
import { CategoryService } from '../../../services/category/category.service';
import { Router } from '@angular/router';
import { Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-add-category',
  standalone: true,
  imports: [GenericFormComponent],
  templateUrl: './add-category.component.html',
  styleUrl: './add-category.component.css',
})
export class AddCategoryComponent {
  formConfig = {
    title: 'Agregar Categoría',
    fields: [
      {
        name: 'name',
        label: 'Nombre',
        type: 'text',
        value: '',
        placeholder: 'Ingrese el nombre de la categoría',
        validators: [Validators.required],
        errorMessage: 'El nombre es requerido',
      },
      {
        name: 'description',
        label: 'Descripción',
        type: 'text',
        value: '',
        placeholder: 'Ingrese la descripción de la categoría',
      },
    ],
  };

  constructor(
    private categoryService: CategoryService,
    private router: Router,
    private toastr: ToastrService
  ) {}

  onSubmit(formData: any): void {
    console.log(formData);
    this.categoryService.createCategory(formData).subscribe(() => {
      this.toastr.success('Categoría creada con éxito', 'Exito');
      this.router.navigate(['/categories']);
    });
  }

  onCancel(): void {
    this.router.navigate(['/categories']);
  }
}

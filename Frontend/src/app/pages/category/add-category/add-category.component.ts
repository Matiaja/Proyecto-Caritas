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
    // Trim whitespace from form data
    formData.name = formData.name.trim();
    formData.description = formData.description?.trim();
    if (!formData.name) {
      this.toastr.error('El nombre no puede estar vacío');
      return;
    }
    this.categoryService.createCategory(formData).subscribe({
      next: () => {
        this.toastr.success('Categoría creada con éxito', 'Exito');
        this.router.navigate(['/categories']);
      },
      error: (error) => {
        if(error.status === 400 && error.error?.message) {
          this.toastr.error(error.error.message, 'Error');
        } else {
          this.toastr.error('Error al crear la categoría', 'Error');
        }
        console.error('Error al crear la categoría:', error);
      },
    });
  }

  onCancel(): void {
    this.router.navigate(['/categories']);
  }
}

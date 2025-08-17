import { Component, OnInit } from '@angular/core';
import { GenericFormComponent } from '../../../shared/components/generic-form/generic-form.component';
import { FormBuilder, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { CategoryService } from '../../../services/category/category.service';
import { FormGroup } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-edit-category',
  standalone: true,
  imports: [GenericFormComponent],
  templateUrl: './edit-category.component.html',
  styleUrl: './edit-category.component.css',
})
export class EditCategoryComponent implements OnInit {
  categoryData: any;
  categoryId!: number;
  form!: FormGroup;

  formConfig = {
    title: 'Editar Categoría',
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
    private toastr: ToastrService,
    private router: Router,
    private route: ActivatedRoute,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.categoryId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadCategoryData();
  }

  loadCategoryData(): void {
    this.categoryService.getCategory(this.categoryId).subscribe((category) => {
      this.categoryData = category;

      this.formConfig = {
        title: 'Editar Categoría',
        fields: [
          {
            name: 'name',
            label: 'Nombre',
            type: 'text',
            value: this.categoryData.name,
            placeholder: 'Ingrese el nombre de la categoría',
            validators: [Validators.required],
            errorMessage: 'El nombre es requerido',
          },
          {
            name: 'description',
            label: 'Descripción',
            type: 'text',
            value: this.categoryData.description,
            placeholder: 'Ingrese la descripción de la categoría',
          },
        ],
      };
    });
  }

  onSubmit(formData: any): void {
    // Trim whitespace from form data
    formData.name = formData.name.trim();
    formData.description = formData.description?.trim();
    if (!formData.name) {
      this.toastr.error('El nombre no puede estar vacío');
      return;
    }
    this.categoryService.updateCategory(this.categoryId, formData).subscribe({
      next: () => {
        this.toastr.success('Categoría editada con éxito', 'Exito!');
        this.router.navigate(['/categories']);
      },
      error: (error) => {
        if(error.status === 400 && error.error?.message) {
          this.toastr.error(error.error.message, 'Error');
        } else {
          this.toastr.error('Error al editar la categoría', 'Error');
        }
        console.error('Error al editar la categoría:', error);
      },
    });
  }

  onCancel(): void {
    this.router.navigate(['/categories']);
  }
}

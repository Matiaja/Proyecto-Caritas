import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { GenericFormComponent } from '../../../shared/components/generic-form/generic-form.component';
import { Router } from '@angular/router';
import { Validators } from '@angular/forms';
import { BreadcrumbComponent } from '../../../shared/components/breadcrumbs/breadcrumbs.component';
import { ProductService } from '../../../services/product/product.service';
import { CategoryService } from '../../../services/category/category.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-product-add',
  standalone: true,
  imports: [GenericFormComponent, BreadcrumbComponent],
  templateUrl: './product-add.component.html',
  styleUrl: './product-add.component.css',
})
export class ProductAddComponent implements OnInit {
  formConfig = {
    title: 'Agregar Producto',
    fields: [
      {
        name: 'name',
        label: 'Nombre',
        type: 'text',
        value: '',
        placeholder: 'Ingrese el nombre del producto',
        validators: [Validators.required],
        errorMessage: 'El nombre es requerido',
      },
      {
        name: 'code',
        label: 'Código',
        type: 'text',
        value: '',
        placeholder: 'Ingrese el código del producto',
        validators: [Validators.required],
        errorMessage: 'El código es requerido',
      },
      {
        name: 'category',
        label: 'Categoría',
        type: 'select',
        value: '',
        placeholder: 'Seleccione una categoría',
        validators: [Validators.required],
        errorMessage: 'La categoría es requerida',
        options: [] as { value: any; label: string }[],
      },
    ],
  };

  constructor(
    private categoryService: CategoryService,
    private productService: ProductService,
    private router: Router,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.categoryService.categories$.subscribe((categories) => {
      const categoryField = this.formConfig.fields.find((field) => field.name === 'category');
      if (categoryField) {
        categoryField.options = categories.map((category) => ({
          value: category.id,
          label: category.name,
        }));
      }
    });

    this.categoryService.getCategories();
  }

  onSubmit(formData: any): void {
    console.log(formData);
    const payload = {
      name: formData.name,
      categoryId: formData.category,
    };
    this.productService.createProduct(payload).subscribe(() => {
      this.toastr.success('Producto creado con éxito', 'Exito');
      this.router.navigate(['/products']);
    });
  }

  onCancel(): void {
    this.router.navigate(['/products']);
  }
}

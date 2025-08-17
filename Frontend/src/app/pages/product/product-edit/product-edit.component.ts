import { Component, OnInit } from '@angular/core';
import { Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { CategoryService } from '../../../services/category/category.service';
import { ProductService } from '../../../services/product/product.service';
import { GenericFormComponent } from "../../../shared/components/generic-form/generic-form.component";

@Component({
  selector: 'app-product-edit',
  standalone: true,
  imports: [GenericFormComponent],
  templateUrl: './product-edit.component.html',
  styleUrl: './product-edit.component.css'
})
export class ProductEditComponent implements OnInit {
  productData: any = null;
  productId!: number;

  formConfig = {
    title: 'Editar producto',
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
        validators: [],
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
    private route: ActivatedRoute,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.productId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadCategories();
    this.loadProductData();
  }

  loadProductData(): void {
    this.productService.getProductById(this.productId).subscribe({
      next: (product) => {
        this.productData = product;

        // Create a new formConfig object with updated values
        this.formConfig = {
          ...this.formConfig,
          fields: this.formConfig.fields.map((field: any) => ({
            ...field,
            value:
              field.name === 'name'
                ? this.productData.name
                : field.name === 'code'
                ? this.productData.code
                : field.name === 'category'
                ? this.productData.categoryId
                : field.value,
          }))};
      },
      error: (error) => {
        this.toastr.error('Producto no encontrado', 'Error');
        console.error('Error loading product data:', error);
        this.onCancel();
      }
    });
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
    const payload = {
      name: formData.name.trim(),
      code: formData.code?.trim() || '',
      categoryId: formData.category,
    };
    if (!payload.name) {
      this.toastr.error('El nombre del producto es obligatorio');
      return;
    }
    this.productService.editProduct(this.productId, payload).subscribe({
      next: () => {
        this.toastr.success('Producto editado con éxito', 'Exito');
        this.router.navigate(['/products']);
      },
      error: (error) => {
        if (error.error && error.error.message) {
          this.toastr.error(error.error.message, 'Error');
        } else {
          this.toastr.error('Error al editar el producto', 'Error');
        }
        console.error('Error on edit product:', error);
      },
    });
  }

  onCancel(): void {
    this.router.navigate(['/products']);
  }
}

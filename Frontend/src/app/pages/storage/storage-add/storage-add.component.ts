import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { GenericFormComponent } from '../../../shared/components/generic-form/generic-form.component';
import { Router } from '@angular/router';
import { Validators } from '@angular/forms';
import { BreadcrumbComponent } from '../../../shared/components/breadcrumbs/breadcrumbs.component';
import { ProductService } from '../../../services/product/product.service';
import { StockService } from '../../../services/stock/stock.service';
import { expirationDateValidator } from '../../../shared/validators/date-compare.validator';

@Component({
  selector: 'app-storage-add',
  standalone: true,
  imports: [GenericFormComponent, BreadcrumbComponent],
  templateUrl: './storage-add.component.html',
  styleUrl: './storage-add.component.css'
})
export class StorageAddComponent implements OnInit{
  formConfig = {
    title: 'Agregar Stock',
    fields: [
      {
        name: 'product',
        label: 'Producto',
        type: 'select',
        value: '',
        placeholder: 'Seleccione un producto',
        validators: [Validators.required],
        errorMessage: 'El producto es requerido',
        options: [] as { value: any; label: string }[],
      },
      {
        name: 'type',
        label: 'Tipo',
        type: 'select',
        value: 'Ingreso',
        placeholder: 'Seleccione el tipo',
        validators: [Validators.required],
        errorMessage: 'El tipo es requerido',
        options: [
          { value: 'Ingreso', label: 'Ingreso' },
          { value: 'Egreso', label: 'Egreso' },
        ],
      },
      {
        name: 'date',
        label: 'Fecha',
        type: 'date',
        value: '',
        placeholder: 'Seleccione la fecha',
        validators: [Validators.required],
        errorMessage: 'La fecha es requerida',
      },
      {
        name: 'expirationDate',
        label: 'Fecha de expiración',
        type: 'date',
        value: '',
        placeholder: 'Seleccione la fecha de expiración',
        validators: [
          expirationDateValidator('date'),
        ],
        errorMessage: 'La fecha de expiracion no puede ser menor a hoy ni a la fecha de creación',
      },
      {
        name: 'description',
        label: 'Descripción',
        type: 'textarea',
        value: '',
        placeholder: 'Ingrese una descripción',
        validators: [],
        errorMessage: '',
      },
      {
        name: 'quantity',
        label: 'Cantidad',
        type: 'number',
        value: '',
        placeholder: 'Ingrese la cantidad',
        validators: [Validators.required, Validators.min(0)],
        errorMessage: 'La cantidad es requerida y debe ser mayor o igual a 0',
      },
      {
        name: 'weight',
        label: 'Peso',
        type: 'number',
        value: '',
        placeholder: 'Ingrese el peso',
        validators: [Validators.min(0)],
        errorMessage: 'El peso es requerido y debe ser mayor o igual a 0',
      },
    ],
  };

  constructor(
    private productService: ProductService,
    private stockService: StockService,
    private router: Router
  ) {}
  
  ngOnInit(): void {
      this.loadProducts();
  }

  loadProducts() {
    this.productService.products$.subscribe(products => {
      this.formConfig.fields[0].options = products.map((product) => {
        return { value: product.id, label: product.name };
      });
    });
    this.productService.getProducts();
  }

  onSubmit(data: any): void {
    const centerId = localStorage.getItem('currentCenterId');
    const payload = {
      productId: data.product,
      type: data.type,
      date: data.date,
      expirationDate: data.expirationDate,
      description: data.description,
      quantity: data.quantity,
      weight: data.weight,
      centerId: centerId,
    };

    this.stockService.createStock(payload).subscribe(() => {
      this.router.navigate(['/storage']);
    });
  }

  onCancel(): void {
    this.router.navigate(['/storage']);
  }

}

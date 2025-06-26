import { Component } from '@angular/core';
import { GenericFormComponent } from '../../../shared/components/generic-form/generic-form.component';
import { Router } from '@angular/router';
import { Validators } from '@angular/forms';
import { BreadcrumbComponent } from '../../../shared/components/breadcrumbs/breadcrumbs.component';
import { ProductService } from '../../../services/product/product.service';
import { StockService } from '../../../services/stock/stock.service';
import { expirationDateValidator } from '../../../shared/validators/date-compare.validator';
import { GlobalStateService } from '../../../services/global/global-state.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-storage-add',
  standalone: true,
  imports: [GenericFormComponent, BreadcrumbComponent],
  templateUrl: './storage-add.component.html',
  styleUrl: './storage-add.component.css',
})
export class StorageAddComponent {
  formConfig = {
    title: 'Agregar Stock',
    fields: [
      {
        name: 'productSearch',
        label: 'Producto',
        type: 'searchProducts',
        value: '',
        placeholder: 'Escriba para buscar un producto...',
        validators: [Validators.required],
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
        validators: [expirationDateValidator('date')],
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
        label: 'Peso (kg)',
        type: 'number',
        value: '',
        placeholder: 'Ingrese el peso',
        validators: [Validators.min(0)],
        errorMessage: 'El peso debe ser mayor o igual a 0',
      },
    ],
  };

  constructor(
    private productService: ProductService,
    private stockService: StockService,
    private router: Router,
    private globalStateService: GlobalStateService,
    private toastr: ToastrService
  ) {}

  onSubmit(data: any): void {
    const centerId = this.globalStateService.getCurrentCenterId();
    const payload = {
      productId: data.productSearch?.id,
      type: data.type,
      date: data.date,
      expirationDate: data.expirationDate !== "" ? data.expirationDate : null,
      description: data.description ? data.description : '',
      quantity: data.quantity,
      weight: data.weight ? data.weight : 0,
      centerId: centerId,
    };

    console.log('Payload to create stock:', payload);

    this.stockService.createStock(payload).subscribe(() => {
      this.toastr.success('Stock creado con éxito', 'Exito');
      this.router.navigate(['/storage']);
    });
  }

  onCancel(): void {
    this.router.navigate(['/storage']);
  }
}

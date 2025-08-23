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
        value: '',
        placeholder: 'Seleccione el tipo',
        validators: [Validators.required],
        errorMessage: 'El tipo es requerido',
        options: [
          { value: 'Ingreso', label: 'Ingreso' },
          { value: 'Egreso', label: 'Egreso' },
        ],
        onValueChange: (value: string) => this.updateOriginLabel(value),
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
        name: 'origin',
        label: 'Origen/Destino', // Se actualizará dinámicamente
        type: 'text',
        value: '',
        placeholder: 'Ingrese el origen o destino del stock',
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


  updateOriginLabel(typeValue: string): void {
    const originField = this.formConfig.fields.find(field => field.name === 'origin');
    if (originField) {
      originField.label = typeValue === 'Ingreso' ? 'Origen' : 'Destino';
      originField.placeholder = typeValue === 'Ingreso' ? 'Ingrese el origen del stock' : 'Ingrese el destino del stock';
    }
  }

  onSubmit(data: any): void {
    const centerId = this.globalStateService.getCurrentCenterId();
    const payload = {
      productId: data.productSearch?.id,
      type: data.type,
      date: data.date ? data.date.toISOString().split('T')[0] : undefined,
      expirationDate: data.expirationDate !== "" ? data.expirationDate.toISOString().split('T')[0] : null,
      description: data.description ? data.description : '',
      origin: data.origin ? data.origin : null, 
      quantity: data.quantity,
      weight: data.weight ? data.weight : 0,
      centerId: centerId,
    };

    this.stockService.createStock(payload).subscribe({
      next: () => {
        this.toastr.success('Stock creado con éxito', 'Exito');
        this.router.navigate(['/storage']);
      },
      error: (err) => {
        console.error('Error creating stock:', err);
        if(err.error && err.error.message) {
          this.toastr.error(err.error.message, 'Error');
        }
        else {
          this.toastr.error('Error al crear el stock', 'Error');
        }
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/storage']);
  }
}

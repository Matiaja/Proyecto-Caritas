import { Component, Inject, Injectable, OnInit, Optional, ViewEncapsulation } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray, ReactiveFormsModule } from '@angular/forms';
import { PurchaseService } from '../../../services/purchase/purchase.service';
import { ProductService } from '../../../services/product/product.service';
import { GlobalStateService } from '../../../services/global/global-state.service';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MAT_DATE_FORMATS, MatNativeDateModule } from '@angular/material/core';
import { MatInputModule } from '@angular/material/input';
import { MAT_DATE_LOCALE, DateAdapter, provideNativeDateAdapter } from '@angular/material/core';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MomentDateAdapter } from '@angular/material-moment-adapter';
import moment from 'moment';

@Injectable()
export class CustomDateAdapter extends MomentDateAdapter {
  constructor(@Optional() @Inject(MAT_DATE_LOCALE) dateLocale: string) {
    super(dateLocale);
  }

  override parse(value: any): moment.Moment | null {
    if (value && typeof value === 'string') {
      // Intenta parsear diferentes formatos
      const formats = ['DD/MM/YYYY', 'DD-MM-YYYY', 'DD.MM.YYYY', 'D/M/YYYY', 'D-M-YYYY'];
      const date = moment(value, formats, true);
      return date.isValid() ? date : null;
    }
    return value ? moment(value, moment.ISO_8601, true) : null;
  }
}

export const MY_DATE_FORMATS = {
  parse: {
    dateInput: 'DD/MM/YYYY',
  },
  display: {
    dateInput: 'DD/MM/YYYY',
    monthYearLabel: 'MMMM YYYY',
    dateA11yLabel: 'LL',
    monthYearA11yLabel: 'MMMM YYYY',
  },
};

@Component({
  selector: 'app-purchase-add',
  standalone: true,
  providers: [
    { provide: DateAdapter, useClass: CustomDateAdapter },
    { provide: MAT_DATE_FORMATS, useValue: MY_DATE_FORMATS },
    { provide: MAT_DATE_LOCALE, useValue: 'es-ES' },
  ],
  imports: [
    MatFormFieldModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatInputModule,
    ReactiveFormsModule,
    CommonModule
  ],
  templateUrl: './purchase-add.component.html',
  styleUrl: './purchase-add.component.css',
  encapsulation: ViewEncapsulation.None
})
export class PurchaseAddComponent implements OnInit {
  today: Date = new Date();
  purchaseForm: FormGroup;
  products: any[] = [];
  filteredProducts: any[][] = [];

  constructor(
    private fb: FormBuilder,
    private purchaseService: PurchaseService,
    private productService: ProductService,
    private toastr: ToastrService,
    private router: Router,
    private globalStateService: GlobalStateService
  ) {
    this.purchaseForm = this.fb.group({
      purchaseDate: [null, [Validators.required]],
      type: ['', [Validators.required]],
      centerId: [0],
      items: this.fb.array([])
    });
  }

  ngOnInit(): void {
    this.purchaseForm.get('centerId')?.setValue(this.globalStateService.getCurrentCenterId());
    this.productService.getProducts().subscribe(data => this.products = data);
  }

  get items(): FormArray {
    return this.purchaseForm.get('items') as FormArray;
  }

  onDateInput(event: any) {
    const input = event.target.value;

    // permitir solo números y borrar
    if (!/^\d*\/?\d*$/.test(input)) {
      event.target.value = input.slice(0, -1);
      return;
    }
    
    // Auto-formateo mientras el usuario escribe
    if (input.length === 2 || input.length === 5) {
      if (event.key !== 'Backspace' && event.key !== 'Delete') {
        event.target.value = input + '/';
      }
    }
  }

  addItem() {
    const itemGroup = this.fb.group({
      productId: [0],
      productName: [''],
      quantity: [1, [Validators.min(1)]],
      description: ['']
    });
    this.items.push(itemGroup);
    this.filteredProducts.push([]);
  }

  removeItem(index: number) {
    this.items.removeAt(index);
    this.filteredProducts.splice(index, 1);
  }

  filterProducts(index: number) {
    const searchTerm = this.items.at(index).get('productName')?.value?.toLowerCase() || '';
    if (!searchTerm.trim() || searchTerm.length < 3) {
      this.filteredProducts[index] = [];
      return;
    }
    this.filteredProducts[index] = this.products.filter(p =>
      p.name.toLowerCase().includes(searchTerm)
    );
  }

  selectProduct(index: number, product: any) {
    this.items.at(index).get('productId')?.setValue(product.id);
    this.items.at(index).get('productName')?.setValue(product.name);
    this.filteredProducts[index] = [];
  }

  submit() {
    this.purchaseForm.markAllAsTouched();

    if (this.items.length === 0) {
      this.toastr.error('Debe agregar al menos un ítem', 'Error');
      return;
    }

    for (const item of this.items.controls) {
      if (!item.get('productId')?.value || item.get('productId')?.value <= 0) {
        this.toastr.error('Todos los ítems deben tener un producto válido', 'Error');
        return;
      }
    }

    const purchase = {
      ...this.purchaseForm.value,
      purchaseDate: this.purchaseForm.value.purchaseDate?.toISOString().split('T')[0]
    };

    this.purchaseService.createPurchase(purchase).subscribe({
      next: (res: any) => {
        this.toastr.success('Compra registrada correctamente', 'Éxito');
        this.router.navigate(['/purchases/' + res.id]);
      },
      error: (err) => {
        console.error(err);
        if (err.error && err.error.message) {
          this.toastr.error(err.error.message, 'Error');
        } else {
          this.toastr.error('Error al guardar la compra', 'Error');
        }
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/purchases']);
  }
}
import { Component, EventEmitter, Input, Output, SimpleChanges, OnChanges, ViewEncapsulation } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { ProductService } from '../../../services/product/product.service';
import { StockService } from '../../../services/stock/stock.service';
import { catchError, switchMap } from 'rxjs/operators';
import { of } from 'rxjs';
import { GlobalStateService } from '../../../services/global/global-state.service';
import { MAT_DATE_LOCALE, MatNativeDateModule, provideNativeDateAdapter } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormField, MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'generic-form',
  standalone: true,
  providers: [{provide: MAT_DATE_LOCALE, useValue: 'es-AR'},
  provideNativeDateAdapter()
  ],
  imports: [ReactiveFormsModule, CommonModule,
    MatFormField,
    MatDatepickerModule,
    MatNativeDateModule,
    MatFormFieldModule,
    MatInputModule,
    FormsModule,],
  templateUrl: './generic-form.component.html',
  styleUrl: './generic-form.component.css',
  encapsulation: ViewEncapsulation.None
})
export class GenericFormComponent implements OnChanges {
  @Input() data!: {
    title: string;
    fields: {
      name: string;
      label: string;
      type: string;
      placeholder?: string;
      value?: any;
      validators?: any[];
      errorMessage?: string;
      options?: { value: any; label: string }[];
      onValueChange?: (value: any) => void;
    }[];
  };

  @Input() showButtons = true;
  @Input() products: any[] = [];

  @Output() formSubmit = new EventEmitter<any>();
  @Output() formCancel = new EventEmitter<void>();
  @Output() formChange = new EventEmitter<FormGroup>();

  form!: FormGroup;
  suggestions: Record<string, any[]> = {};

  constructor(
    private fb: FormBuilder,
    private productService: ProductService,
    private stockService: StockService,
    private globalStateService: GlobalStateService
  ) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['data'] && this.data) {
      this.initializeForm();
      this.formChange.emit(this.form);
      this.addAsyncValidatorsForEgreso();
    }
  }

  private initializeForm(): void {
    const controls: Record<string, any> = {};
    this.data.fields.forEach((field) => {
      controls[field.name] = [
        field.value || '',
        field.validators ? Validators.compose(field.validators) : [],
      ];
    });
    this.form = this.fb.group(controls);

    // Suscribirse a valueChanges de los campos con onValueChange
    this.data.fields.forEach((field) => {
      if (this.form.get(field.name)) {
        this.form.get(field.name)!.valueChanges.subscribe((value: any) => {
          field.onValueChange?.(value);
        });
      }
    });
  }

  private addAsyncValidatorsForEgreso(): void {
    const typeControl = this.form.get('type');
    const quantityControl = this.form.get('quantity');

    if (typeControl && quantityControl) {
      typeControl.valueChanges.subscribe((type: string) => {
        if (type === 'Egreso') {
          quantityControl.setAsyncValidators(this.validateStockQuantity());
        } else {
          quantityControl.clearAsyncValidators();
        }
        quantityControl.updateValueAndValidity();
      });
    }
  }

  private validateStockQuantity() {
    return (control: any) => {
      const productId = this.form.get('productSearch')?.value?.id;
      const centerId = this.globalStateService.getCurrentCenterId();
      const newQuantity = control.value;

      
      return this.stockService.validateQuantity(centerId!, productId, newQuantity, "Egreso").pipe(
        switchMap(() => {
          return of(null);
        }),
        catchError((error) => {
          console.log('Error validating quantity:', error);
          return of({ quantityInvalid: error.error.message });
        })
      );
    };
  }

  onSearchChange(event: Event, value: string) {
    const searchTerm = (event.target as HTMLInputElement).value?.toLowerCase();
    if (!searchTerm) {
      this.suggestions[value] = [];
      return;
    }
    // Buscar localmente en this.products
    const filtered = this.products
      .filter(product => product.name?.toLowerCase().includes(searchTerm))
      .map(product => ({ value: product.id, label: product.name }));
    this.suggestions[value] = filtered;
  }

  selectSearchResult(field: string, value: any) {
    this.form.get(field)?.setValue({ id: value.value, name: value.label });
    this.suggestions[field] = [];
  }

  getProductLabel(fieldName: string): string {
    const fieldValue = this.form.get(fieldName)?.value;
    return fieldValue && fieldValue.name ? fieldValue.name : '';
  }

  onSubmit(): void {
    if (this.form.valid) {
      this.formSubmit.emit(this.form.value);
    }
  }

  onCancel(): void {
    this.formCancel.emit();
  }
  
  public isRequired(field: any): boolean {
    // Checks if the validators array exists and includes Validators.required
    return field.validators && field.validators.includes(Validators.required);
  }
}

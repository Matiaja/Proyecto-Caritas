import { Component, EventEmitter, Input, Output, SimpleChanges, OnChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { ProductService } from '../../../services/product/product.service';
import { StockService } from '../../../services/stock/stock.service';
import { catchError, switchMap } from 'rxjs/operators';
import { of } from 'rxjs';
import { GlobalStateService } from '../../../services/global/global-state.service';

@Component({
  selector: 'generic-form',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './generic-form.component.html',
  styleUrl: './generic-form.component.css',
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
    }[];
  };

  @Input() showButtons = true;

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

      return this.stockService.validateQuantity(centerId!, productId, newQuantity).pipe(
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
    const searchTerm = (event.target as HTMLInputElement).value;
    if (!searchTerm) {
      this.suggestions[value] = [];
      return;
    }
    this.productService.searchProducts(searchTerm).subscribe(
      (products) => {
        this.suggestions[value] = products.map((product: any) => ({
          value: product.id,
          label: product.name,
        }));
      },
      (error) => {
        console.error('Error searching products:', error);
        this.suggestions[value] = [];
      }
    );
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
}

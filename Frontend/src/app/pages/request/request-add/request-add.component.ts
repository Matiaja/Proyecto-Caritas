import { Component, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { Validators, FormsModule, ReactiveFormsModule, FormGroup, FormBuilder } from '@angular/forms';
import { GenericFormComponent } from '../../../shared/components/generic-form/generic-form.component';
import { Router } from '@angular/router';
import { CenterService } from '../../../services/center/center.service';
import {MatInputModule} from '@angular/material/input';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatStepper, MatStepperModule} from '@angular/material/stepper';
import {MatButtonModule} from '@angular/material/button';
import { OrderLine } from '../../../models/orderLine.model';
import { RequestModel } from '../../../models/request.model';
import { CommonModule, Location } from '@angular/common';
import { ProductService } from '../../../services/product/product.service';
import { UiTableComponent } from '../../../shared/components/ui-table/ui-table.component';
import { StepperSelectionEvent } from '@angular/cdk/stepper';
import { Product } from '../../../models/product.model';
import { HttpClient } from '@angular/common/http';
import { RequestService } from '../../../services/request/request.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-request-add',
  standalone: true,
  imports: [GenericFormComponent, 
    MatButtonModule,
    MatStepperModule,
    FormsModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    CommonModule,
    UiTableComponent
  ],
  templateUrl: './request-add.component.html',
  styleUrl: './request-add.component.css',
  encapsulation: ViewEncapsulation.None,
})
export class RequestAddComponent implements OnInit {
  @ViewChild('stepper') stepper!: MatStepper;
  isLinear: boolean = true;

  // formularios
  formGroup1!: FormGroup;
  formGroup2!: FormGroup;
  
  // variables del form
  centers: any[] = [];
  products: Product[] = [];
  request: RequestModel = {
    requestingCenterId: 0,
    urgencyLevel: '',
    requestDate: '',
    orderLines: []
  } as RequestModel;
  productSuggestions: any[] = [];
  selectedProduct: any = null;

  // variables de la tabla
  orderLines: OrderLine[] = []; 
  title = 'Lista de pedidos';
    columnHeaders: { [key: string]: string } = {
      productName: 'Producto',
      quantity: 'Cantidad',
      description: 'Descripcion',
    };
  displayedColumns = ['productName', 'quantity', 'description'];
  onAddRequest() {}
  onEditRequest() {}
  onSelectRequest() {}
  onDeleteRequest3() {}
  onDeleteRequest(orderLine: OrderLine) {
    const index = this.orderLines.findIndex(ol => ol === orderLine);
    console.log(index);
    if (index > -1) {
      this.orderLines = this.orderLines.filter(ol => ol !== orderLine);
    }
    if(this.orderLines.length <= 0) {
      this.isLinear = true;
    }
  }

  // configuracion del primer formulario
  formConfig = {
    title : 'Agregar solicitud',
      fields: [
        {
          name: 'requestingCenterId',
          label: 'Centro solicitante',
          type: 'select',
          value: '',
          placeholder: 'Seleccione el centro',
          validators: [Validators.required],
          errorMessage: 'El centro es requerido',
          options: [],
        },
        {
          name: 'urgencyLevel',
          label: 'Nivel de urgencia',
          type: 'select',
          value: '',
          placeholder: 'Seleccione la urgencia',
          validators: [Validators.required],
          errorMessage: 'Este campo es requerido',
          options: ['Bajo', 'Alto'].map((option) => ({
            value: option,
            label: option,
          })),
        },
      ],
    };

  constructor(
    private router: Router, 
    private centerService: CenterService, 
    private fb: FormBuilder,
    private productService: ProductService,
    private requestService: RequestService,
    private location: Location,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.formGroup1 = new FormGroup({});
    this.formGroup2 = this.fb.group({
      product: [null, Validators.required],
      quantity: [null, [Validators.required, Validators.min(1)]],
      description: [null, Validators.required]
    });
    this.loadProducts();
    this.loadCenters();    
  }

  loadProducts(): void {
    this.productService.getProducts().subscribe({
      next: (products: Product[]) => {
        this.products = products;
      },
      error: (error) => {
        console.log(error);
      }
    });
  }

  loadCenters(): void {
    // Load centers from API
    this.centerService.getCenters().subscribe({
      next: (centers) => {
        this.centers = centers.map((center) => ({
            value: center.id,
            label: center.name,
          }));
        this.formConfig.fields[0].options = this.centers;
      },
      error: (error) => {
        console.error(error);
      }
    });
  }

  // Buscar producto
  searchProduct(event: Event): void {
    const input = event.target as HTMLInputElement; // Casting explícito
    const searchTerm = input.value;
    if (searchTerm.length > 1) {
      this.productSuggestions = this.products.filter(prod => 
        prod.name.toLowerCase().includes(searchTerm.toLowerCase())
      );
    } else {
      this.productSuggestions = [];
    }
  }

  // Seleccionar producto
  selectProduct(product: any): void {
    this.selectedProduct = product;
    this.formGroup2.get('product')?.setValue(product.name);
    this.productSuggestions = [];
  }


  // Agregar un producto a la lista de pedidos
  addOrderLine(): void {
    const productId = this.selectedProduct.id;
    const quantity = this.formGroup2.get('quantity')?.value;
    const description = this.formGroup2.get('description')?.value;

    const ol: OrderLine = {
      productId,
      quantity,
      description,
      productName: this.selectedProduct.name
    }

    if (this.formGroup2.valid) {
      this.orderLines = [...this.orderLines, ol];
      this.isLinear = false;
      this.formGroup2.reset(); // Limpiar el formulario
      this.selectedProduct = null;
    }
  }

  makeRequest(event: StepperSelectionEvent) {
    if(event.selectedIndex === 2) {
      const center = this.centers.find(center => center.value == this.formGroup1.get('requestingCenterId')?.value)
      this.request = {
        ...this.formGroup1.value,
        requestDate: new Date().toISOString(),
        orderLines: this.orderLines,
        requestingCenter: {
          id: center.value,
          name: center.label
        }
      };
    }
  }

  goToNextStep() {
    if(this.orderLines.length !== 0) {
      this.stepper.next();
    }
  }

  next(form: FormGroup): void {
    if(form.valid) {
      this.request = {
        ...this.request,
        ...form.value
      };
      this.stepper.next();
    }
  }

  // Confirmar y enviar solicitud
  finalizeRequest(): void {
    const center = this.centers.find(center => center.value == this.formGroup1.get('requestingCenterId')?.value)

    const requestDTO = {
      ...this.formGroup1.value,
      requestDate: new Date().toISOString(),
      orderLines: this.orderLines
    };
    // console.log(this.request);
    this.requestService.addRequest(requestDTO).subscribe({
      next: (response) => {
        this.toastr.success('Solicitud creada con éxito', 'Éxito');
        console.log('Solicitud creada con éxito', response);
        this.router.navigate(['requests/']);
      },
      error: (err) => {
        console.error('Error al crear la solicitud', err);
        this.toastr.error('Hubo un error al crear la solicitud. Inténtalo nuevamente.', 'Error');
        alert('Hubo un error al crear la solicitud. Inténtalo nuevamente.');
      },
    });
  }

  goBack() {
    this.location.back();
  }

  
  onSubmit(): void {}
  onCancel(): void {
    this.router.navigate(['/requests']);
  }

}

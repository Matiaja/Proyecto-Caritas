import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PurchaseService } from '../../../services/purchase/purchase.service';
import { DistributionService } from '../../../services/distribution/distribution.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CenterService } from '../../../services/center/center.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-distribution',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './distribution.component.html',
  styleUrl: './distribution.component.css'
})
export class DistributionComponent implements OnInit {
  purchaseId!: number;
  purchase!: any;

  destinationType: 'center' | 'person' = 'center';
  destinationValue = '';
  destinationPerson = '';
  destinationDNI = '';
  destinationMemberFamily = '';
  destinationLocation = '';

  items: { item: any; quantityToDeliver: number; description: string }[] = [];

  centers: any[] = [];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private purchaseService: PurchaseService,
    private distributionService: DistributionService,
    private centerService: CenterService,
    private toast: ToastrService
  ) {}

  ngOnInit(): void {
    this.purchaseId = Number(this.route.snapshot.paramMap.get('id'));
    this.purchaseService.getById(this.purchaseId).subscribe(p => {
      this.purchase = p;
      this.items = p.items.map((i: any) => ({ item: i, quantityToDeliver: 0 }));
    });
    // agregar centers
    this.centerService.getCenters().subscribe(centers => {
      this.centers = centers;
    });
  }

  // Permitir solo números en inputs
  onlyNumbers(event: any): void {
    const input = event.target.value;
    if (!/^\d*$/.test(input + event.key) && event.key !== 'Backspace') {
      event.preventDefault();
    }
  }

  confirmDistribution(): void {
    // Validaciones de destino
    if (this.destinationType === 'center') {
      if (!this.destinationValue) {
        this.toast.error("Debe seleccionar un centro de destino.", "Error");
        return;
      }
    }
    if (!this.destinationPerson.trim()) {
      this.toast.error("Debe ingresar el nombre de la persona que recibe.", "Error");
      return;
    }
    if (!this.destinationDNI.trim()) {
      this.toast.error("Debe ingresar el DNI de la persona.", "Error");
      return;
    }
    if (!this.destinationLocation.trim()) {
      this.toast.error("Debe ingresar la localidad/dirección de la persona.", "Error");
      return;
    }
    if (this.destinationType === 'person') {
      if (!this.destinationMemberFamily.trim()) {
        this.toast.error("Debe ingresar el tipo de miembro de la familia de la persona.", "Error");
        return;
      }
    }

    // Validaciones de ítems
    const selectedItems = this.items.filter(i => i.quantityToDeliver > 0);

    if (selectedItems.length === 0) {
      this.toast.error("Debe entregar al menos un ítem.", "Error");
      return;
    }

    for (const row of selectedItems) {
      if (row.quantityToDeliver > row.item.remainingQuantity) {
        this.toast.error(
          `La cantidad de "${row.item.productName}" excede lo disponible (${row.item.remainingQuantity}).`,
          "Error"
        );
        return;
      }
    }
    // Armar objeto para enviar
    const distribution: any = {
      purchaseId: this.purchaseId,
      centerId: this.destinationType === 'center' ? Number(this.destinationValue) : null,
      personName: this.destinationPerson ? this.destinationPerson.trim() : null,
      personDNI: this.destinationDNI ? this.destinationDNI.trim() : null,
      personMemberFamily: this.destinationType === 'person' ? this.destinationMemberFamily : null,
      personLocation: this.destinationLocation ? this.destinationLocation.trim() : null,
      items: selectedItems.map(i => ({
        itemPurchaseId: i.item.id,
        quantity: i.quantityToDeliver,
        description: i.description || ''
      }))
    };
    // Enviar al backend
    this.distributionService.create(distribution).subscribe({
      next: () => {
        this.toast.success("Entrega registrada correctamente.", "Éxito");
        this.router.navigate(['/purchases/', this.purchaseId]);
      },
      error: (err) => {
        console.error('Error al crear la distribución:', err);
        if (err.error && err.error.message) {
          this.toast.error(err.error.message, 'Error');
        } else {
          this.toast.error('Error al crear la distribución. Por favor, inténtelo de nuevo.', 'Error');
        }
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/purchases']);
  }
}

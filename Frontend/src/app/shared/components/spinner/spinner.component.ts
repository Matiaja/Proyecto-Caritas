import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-spinner',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './spinner.component.html',
  styleUrl: './spinner.component.css'
})
export class SpinnerComponent {
  /** Mensaje opcional bajo el spinner */
  @Input() message: string | null = null;
  /** Toggle para usar imagen vs css spinner */
  @Input() useImage = true;
}

import { Component, Inject, Input } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { FormBuilder, FormGroup } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'generic-form-modal',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    CommonModule,
    MatDialogModule,
    MatIconModule
  ],
  templateUrl: './generic-form-modal.component.html',
  styleUrl: './generic-form-modal.component.css'
})
export class GenericFormModalComponent {
  form: FormGroup;

  constructor(
    private dialogRef: MatDialogRef<GenericFormModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { fields: any[]; title: string },
    private fb: FormBuilder
  )
  {
    this.form = this.fb.group({});
    this.createFormFields(data.fields);
  }

  private createFormFields(fields: any[]): void {
    fields.forEach((field) => {
      this.form.addControl(
        field.name,
        this.fb.control(field.value || '', field.validators || [])
      );
    });
  }

  onSubmit(): void {
    if (this.form.valid) {
      this.dialogRef.close(this.form.value);
    }
  }

  onCancel(): void {
    this.dialogRef.close(null);
  }

}

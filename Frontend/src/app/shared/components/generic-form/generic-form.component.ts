import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'generic-form',
  standalone: true,
  imports: [ ReactiveFormsModule, CommonModule ],
  templateUrl: './generic-form.component.html',
  styleUrl: './generic-form.component.css'
})
export class GenericFormComponent implements OnInit {
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

  @Output() formSubmit = new EventEmitter<any>();
  @Output() formCancel = new EventEmitter<void>();

  form!: FormGroup;

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  private initializeForm(): void {
    const controls: { [key: string]: any } = {};
    this.data.fields.forEach((field) => {
      controls[field.name] = [
        field.value || '',
        field.validators ? Validators.compose(field.validators) : [],
      ];
    });
    this.form = this.fb.group(controls);
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

import { Component } from '@angular/core';
import { inject } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../auth/auth.service';
import { EmailValidator, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { passwordMatchValidator } from '../../shared/validators/password-match.validator';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [ReactiveFormsModule, RouterModule, CommonModule],
  templateUrl: './signup.component.html',
  styleUrl: './signup.component.css'
})
export class SignupComponent {
  authService = inject(AuthService);
  router = inject(Router);

  public signupForm = new FormGroup({
    firstname: new FormControl('', [Validators.required]),
    lastname: new FormControl('', [Validators.required]),
    username: new FormControl('', [Validators.required]),
    phone: new FormControl('', [Validators.required]),
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', [Validators.required]),
    confirmPassword: new FormControl('', [Validators.required])
    },
    { validators: passwordMatchValidator }
  );

  public onSubmit() {
    if (this.signupForm.invalid) {
      console.log('Formulario inválido', this.signupForm.errors);
      return;
    }

    if (this.signupForm.valid) {
      console.log('Form Submitted!', this.signupForm.value);
      this.authService.signup(this.signupForm.value)
        .subscribe({
          next: (data: any) => {
            console.log('Data: ', data);
            this.router.navigate(['/login']);
          },
          error: (error: any) => console.log('Error: ', error)
        });
    }
  }
}

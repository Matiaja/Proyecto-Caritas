import { Component } from '@angular/core';
import { inject } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../auth/auth.service';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { GlobalStateService } from '../../services/global/global-state.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterModule, CommonModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent {
  constructor(private globalStateService: GlobalStateService) {}

  authService = inject(AuthService);
  router = inject(Router);
  errorMessage: string | null = null;

  protected loginForm = new FormGroup({
    username: new FormControl('', [Validators.required]),
    password: new FormControl('', [Validators.required]),
  });

  onSubmit() {
    this.authService.login(this.loginForm.value).subscribe({
      next: (data: any) => {
        if (data && data.token) {
          this.globalStateService.setCenterId(data.centerId);
          this.globalStateService.setUserId(data.userId);
          localStorage.setItem('authUser', JSON.stringify(data)); // Almacena el token
          this.router.navigate(['/home']);
          this.errorMessage = null; // Reset error message on successful login
        }
      },
      error: (error) => {
        console.error('Login error:', error);
        this.errorMessage =
          'Credenciales incorrectas.<br>Por favor, verifica tu usuario y contraseña.';
      },
    });
  }
}

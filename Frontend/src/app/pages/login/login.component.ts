import { Component, inject } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../auth/auth.service';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { GlobalStateService } from '../../services/global/global-state.service';
import { NotificationService } from '../../services/notification/notification.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterModule, CommonModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent {
  // --- Dependency Injection ---
  private authService = inject(AuthService);
  private router = inject(Router);
  private globalStateService = inject(GlobalStateService);
  private notificationService = inject(NotificationService);

  // --- Component Properties ---
  errorMessage: string | null = null;
  passwordVisible: boolean = false; // <-- Visibility property for the password field

  protected loginForm = new FormGroup({
    username: new FormControl('', [Validators.required]),
    password: new FormControl('', [Validators.required]),
  });

  // --- Component Logic ---
  onSubmit() {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched(); // Mark all fields as touched to display validation errors
      return;
    }

    this.authService.login(this.loginForm.value).subscribe({
      next: (data: any) => {
        if (data && data.token) {
          this.globalStateService.setCenterId(data.centerId);
          this.globalStateService.setUserId(data.userId);

          localStorage.setItem('authUser', JSON.stringify(data));
          this.notificationService.reinitializeForCurrentUser();

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

import { Component } from '@angular/core';
import { inject } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../auth/auth.service';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ ReactiveFormsModule, RouterModule ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {

  authService = inject(AuthService);
  router = inject(Router);

  protected loginForm = new FormGroup({
    username: new FormControl('', [Validators.required]),
    password: new FormControl('', [Validators.required])
  });

  onSubmit() {
    console.log('Form Submitted!', this.loginForm.value);
    this.authService.login(this.loginForm.value).subscribe({
      next: (data: any) => {
        if (data && data.token) {
          localStorage.setItem('authUser', JSON.stringify(data)); // Almacena el token
          this.router.navigate(['/home']);
        }
      },
      error: (error) => {
        console.error('Login error:', error);
      },
  });

    // if (this.loginForm.valid) {
    //   console.log('Form Submitted!', this.loginForm.value);
    //   this.authService.login(this.loginForm.value)
    //   .subscribe(
    //     (data: any) => {
    //       if(this.authService.isLoggedIn()) {
    //         this.router.navigate(['/admin']);
    //       }
    //     console.log('Data: ', data);
    //   });
    // }
  }
}

import { CommonModule } from '@angular/common';
import { Component, HostListener } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../auth/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent {
  constructor(private router: Router, private authService: AuthService) {}

  isVisible: boolean = true; // Controla si el navbar es visible
  lastScrollPosition: number = 0; // Almacena la última posición del scroll

  @HostListener('window:scroll', [])
  onScroll() {
    const currentScrollPosition = window.pageYOffset;

    if (currentScrollPosition > this.lastScrollPosition && currentScrollPosition > 50) {
      // Al desplazarse hacia abajo, oculta el navbar
      this.isVisible = false;
    } else {
      // Al desplazarse hacia arriba, muestra el navbar
      this.isVisible = true;
    }

    this.lastScrollPosition = currentScrollPosition;
  }

  checkUserRole(role: string): boolean {
    return this.authService.getUserRole() === role;
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']); // Redirigir al login
  }

  editUser() {
    this.router.navigate(['/users/edit']);
  }

  onUpdateProfile() {
    this.router.navigate(['/profile']);
  }

}

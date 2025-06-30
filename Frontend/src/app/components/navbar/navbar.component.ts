import { CommonModule } from '@angular/common';
import { Component, HostListener, } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../auth/auth.service';
import { NotificationComponent } from "../../shared/components/notification/notification.component";
import { filter } from 'rxjs';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, NotificationComponent],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css',
})
export class NavbarComponent  {
  constructor(
    private router: Router,
    private authService: AuthService
  ) {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      this.collapseNavbar();
    });
  
    // Escuchar clics en el documento para colapsar el navbar
    document.addEventListener('click', (event) => this.collapseNavbar(event));
  
  }

  isVisible = true; // Controla si el navbar es visible
  lastScrollPosition = 0; // Almacena la última posición del scroll

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

  collapseNavbar(event?: Event) {
    const clickedElement = event?.target as HTMLElement;
    if (clickedElement?.closest('.dropdown-toggle')) {
      return; // No hagas nada si es el dropdown
    }
  
    // Cierra el navbar cambiando las clases
    const navbar = document.getElementById('navbarNav');
    if (navbar?.classList.contains('show')) {
      navbar.classList.remove('show');
    }
  }
}

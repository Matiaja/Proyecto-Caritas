import { Component, computed } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { NavbarComponent } from './components/navbar/navbar.component';
import { BreadcrumbComponent } from './shared/components/breadcrumbs/breadcrumbs.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, NavbarComponent, BreadcrumbComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent {
  title = 'Frontend';
  showNavbar = true;

  constructor(private router: Router) {
    // escuchar cambios de ruta
    this.router.events.subscribe(() => {
      this.updateNavbarVisibility();
    });
  }

  private updateNavbarVisibility() {
    const excludedRoutes = ['/login']; // rutas sin el navbar
    this.showNavbar = !excludedRoutes.includes(this.router.url);
  }
}

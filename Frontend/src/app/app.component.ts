import { Component } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { NavbarComponent } from "./components/navbar/navbar.component";
import { BreadcrumbComponent } from "./shared/components/breadcrumbs/breadcrumbs.component";

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, NavbarComponent, BreadcrumbComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'Frontend';
  showNavbar = true;

  constructor(private router: Router) {
    // Escuchar cambios de ruta
    this.router.events.subscribe(() => {
      this.updateNavbarVisibility();
    });
  }

  private updateNavbarVisibility() {
    // Condicionar la visibilidad según la ruta activa
    const excludedRoutes = ['/login']; // Rutas donde no se muestra el navbar
    this.showNavbar = !excludedRoutes.includes(this.router.url);
  }
}

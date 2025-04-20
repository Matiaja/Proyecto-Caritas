import { Component, OnInit  } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { NavbarComponent } from './components/navbar/navbar.component';
import { BreadcrumbComponent } from './shared/components/breadcrumbs/breadcrumbs.component';
import { GlobalStateService } from './services/global/global-state.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, NavbarComponent, BreadcrumbComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent implements OnInit {
  title = 'Frontend';
  showNavbar = true;

  constructor(private router: Router, private globalStateService: GlobalStateService) {
    // escuchar cambios de ruta
    this.router.events.subscribe(() => {
      this.updateNavbarVisibility();
    });
  }

  ngOnInit(): void {
      const authUser = localStorage.getItem('authUser');
      if (authUser) {
        const userData = JSON.parse(authUser);
        this.globalStateService.setCenterId(userData.centerId);
        this.globalStateService.setUserId(userData.userId);
      }
  }

  private updateNavbarVisibility() {
    const excludedRoutes = ['/login']; // rutas sin el navbar
    this.showNavbar = !excludedRoutes.includes(this.router.url);
  }
}

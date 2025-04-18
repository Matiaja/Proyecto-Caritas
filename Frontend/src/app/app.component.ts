import { Component, computed } from '@angular/core';
import { Router, RouterOutlet, NavigationEnd, ActivatedRoute } from '@angular/router';
import { NavbarComponent } from "./components/navbar/navbar.component";
import { BreadcrumbComponent } from "./shared/components/breadcrumbs/breadcrumbs.component";
import { filter } from 'rxjs/operators';

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

  constructor(private router: Router, private route: ActivatedRoute) {
    this.router.events.pipe(
        filter(event => event instanceof NavigationEnd)
      ).subscribe(() => {
        this.updateNavbarVisibility();
    });
  }

  private updateNavbarVisibility() {
    let currentRoute = this.route.root;
    while (currentRoute.firstChild) {
      currentRoute = currentRoute.firstChild;
    }
    const hideNavbar = currentRoute.snapshot.data['hideNavbar'];
    this.showNavbar = !hideNavbar;
  }
}

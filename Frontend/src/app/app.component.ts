import { Router, RouterOutlet, NavigationEnd, ActivatedRoute } from '@angular/router';
import { filter } from 'rxjs/operators';
import { Component, OnInit } from '@angular/core';
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

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private globalStateService: GlobalStateService
  ) {
    this.router.events.pipe(filter((event) => event instanceof NavigationEnd)).subscribe(() => {
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
    let currentRoute = this.route.root;
    while (currentRoute.firstChild) {
      currentRoute = currentRoute.firstChild;
    }
    const hideNavbar = currentRoute.snapshot.data['hideNavbar'];
    this.showNavbar = !hideNavbar;
  }
}

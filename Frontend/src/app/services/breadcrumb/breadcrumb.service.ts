import { Injectable } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs/operators';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class BreadcrumbService {
  private _breadcrumbs$ = new BehaviorSubject<{ label: string; url: string }[]>([]);
  breadcrumbs$ = this._breadcrumbs$.asObservable();

  constructor(private router: Router, private activatedRoute: ActivatedRoute) {
    this.router.events.pipe(filter(event => event instanceof NavigationEnd)).subscribe(() => {
      const root = this.activatedRoute.root;
      const breadcrumbs = this.createBreadcrumbs(root);
      this._breadcrumbs$.next(breadcrumbs);
    });
  }

  private createBreadcrumbs(
    route: ActivatedRoute,
    url: string = '',
    breadcrumbs: { label: string; url: string }[] = []
  ): { label: string; url: string }[] {
    const children: ActivatedRoute[] = route.children;

    for (const child of children) {
      const routeURL: string = child.snapshot.url.map(segment => segment.path).join('/');
      if (routeURL !== '') {
        url += `/${routeURL}`;
      }

      const label = child.snapshot.data['breadcrumb'];
      if (label) {
        breadcrumbs.push({ label, url });
      }

      return this.createBreadcrumbs(child, url, breadcrumbs);
    }

    return breadcrumbs;
  }
}
import { Component, OnInit } from '@angular/core';
import { BreadcrumbService } from '../../../services/breadcrumb/breadcrumb.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-breadcrumbs',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './breadcrumbs.component.html',
  styleUrl: './breadcrumbs.component.css',
})
export class BreadcrumbComponent implements OnInit {
  breadcrumbs$: typeof this.breadcrumbService.breadcrumbs$;

  constructor(private breadcrumbService: BreadcrumbService) {
    this.breadcrumbs$ = this.breadcrumbService.breadcrumbs$;
  }

  ngOnInit(): void {}
}

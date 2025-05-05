import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CenterService } from '../../../services/center/center.service';
import { CenterModel } from '../../../models/center.model';
import { CommonModule, Location } from '@angular/common';
import { BreadcrumbComponent } from '../../../shared/components/breadcrumbs/breadcrumbs.component';

@Component({
  selector: 'app-center-detail',
  standalone: true,
  imports: [CommonModule, BreadcrumbComponent],
  templateUrl: './center-detail.component.html',
  styleUrl: './center-detail.component.css',
})
export class CenterDetailComponent implements OnInit {
  center: CenterModel = {
    id: 0,
    name: '',
    location: '',
    manager: '',
    capacityLimit: 0,
    phone: '',
    email: '',
  } as CenterModel;

  constructor(
    private centerService: CenterService,
    private route: ActivatedRoute,
    private router: Router,
    private location: Location
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      const centerId = params['id'];
      this.loadCenterDetails(centerId);
    });
  }

  loadCenterDetails(centerId: any): void {
    this.centerService.getCenterById(centerId).subscribe({
      next: (center: CenterModel) => {
        this.center = center;
        console.log(this.center);
      },
      error: (err) => {
        console.log(err);
      },
    });
  }

  goBack() {
    this.location.back();
  }
}

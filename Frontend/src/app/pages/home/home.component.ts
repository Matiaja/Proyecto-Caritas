import { Component, OnDestroy, OnInit } from '@angular/core';
import { ResponsiveService } from '../../services/responsive/responsive.service';
import { GlobalStateService } from '../../services/global/global-state.service';
import { SafeUrlPipe } from '../../pipes/safe-url.pipe';
import { Subscription } from 'rxjs';
import { AuthService } from '../../auth/auth.service';
import { CenterService } from '../../services/center/center.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [SafeUrlPipe, CommonModule, FormsModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
})
export class HomeComponent implements OnInit, OnDestroy {
  isMobile: boolean = false;
  lookerUrl: string = '';
  private resizeSub!: Subscription;
  isAdmin = false;
  centers: any[] = [];
  selectedCenterId: number | null = null;

  constructor(
    private responsiveService: ResponsiveService,
    private globalStateService: GlobalStateService,
    private authService: AuthService,
    private centerService: CenterService
  ) {}

  ngOnInit() {
    this.resizeSub = this.responsiveService.isMobile$.subscribe((isMobile) => {
      this.isMobile = isMobile;
    });

    this.isAdmin = this.authService.isAdmin();
    console.log('Is Admin:', this.isAdmin);

    const centerId = this.globalStateService.getCurrentCenterId();


    if (this.isAdmin) {
      this.centerService.getCenters().subscribe(centers => {
        this.centers = centers;
        this.selectedCenterId = centers[0]?.id ?? null;
        this.loadCharts();
      });
    } else {
      this.selectedCenterId = this.authService.getUserCenterId?.() ?? this.globalStateService.getCurrentCenterId();
      this.loadCharts();
    }
  }

  loadCharts() {
    console.log('Loading charts for center ID:', this.selectedCenterId);
    if (this.selectedCenterId) {
      const paramObj = {
  "ds3.currentcenterid_": Number(this.selectedCenterId)
};
const encodedParams = encodeURIComponent(JSON.stringify(paramObj));
console.log('Encoded Params:', encodedParams);
this.lookerUrl = `https://lookerstudio.google.com/embed/reporting/0b4bfdae-2d42-48fe-b338-e3de353cead8/page/GbFKF?params=${encodedParams}`;
    }
  }

  onCenterChange() {
    this.loadCharts();
  }

  ngOnDestroy() {
    if (this.resizeSub) {
      this.resizeSub.unsubscribe();
    }
  }
}

import { Component, OnDestroy, OnInit } from '@angular/core';
import { ResponsiveService } from '../../services/responsive/responsive.service';
import { GlobalStateService } from '../../services/global/global-state.service';
import { SafeUrlPipe } from '../../pipes/safe-url.pipe';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [SafeUrlPipe],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
})
export class HomeComponent implements OnInit, OnDestroy {
  isMobile: boolean = false;
  lookerUrl: string = '';
  private resizeSub!: Subscription;

  constructor(
    private responsiveService: ResponsiveService,
    private globalStateService: GlobalStateService
  ) {}

  ngOnInit() {
    this.resizeSub = this.responsiveService.isMobile$.subscribe((isMobile) => {
      this.isMobile = isMobile;
    });

    const centerId = this.globalStateService.getCurrentCenterId();

    // Generar la URL con el parámetro
    this.lookerUrl = `https://lookerstudio.google.com/embed/reporting/0b4bfdae-2d42-48fe-b338-e3de353cead8/page/GbFKF?params=currentCenterId:${centerId}`;
  }

  ngOnDestroy() {
    if (this.resizeSub) {
      this.resizeSub.unsubscribe();
    }
  }
}

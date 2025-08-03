import { Injectable } from '@angular/core';
import { BehaviorSubject, debounceTime, fromEvent } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ResponsiveService {
  private mobileBreakpoint = 576;
  isMobile$ = new BehaviorSubject<boolean>(false);

  constructor() {
    this.checkScreenSize();
    fromEvent(window, 'resize')
      .pipe(debounceTime(100))
      .subscribe(() => this.checkScreenSize());
  }

  private checkScreenSize() {
    const isMobile = window.innerWidth < this.mobileBreakpoint;
    this.isMobile$.next(isMobile);
  }

  // private readonly breakpoints = {
  //   small: '(max-width: 599px)',
  //   medium: '(min-width: 600px) and (max-width: 959px)',
  //   large: '(min-width: 960px)'
  // };

  // private breakpointObserver = inject(BreakpointObserver);

  // isSmallScreen = toSignal(
  //   this.breakpointObserver.observe(this.breakpoints.small)
  // );

  // isMediumScreen = toSignal(
  //   this.breakpointObserver.observe(this.breakpoints.medium)
  // );

  // isLargeScreen = toSignal(
  //   this.breakpointObserver.observe(this.breakpoints.large)
  // );

  // currentScreenSize() {
  //   if (this.isSmallScreen()) return 'small';
  //   if (this.isMediumScreen()) return 'medium';
  //   return 'large';
  // }
}

import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class GlobalStateService {
  private centerIdSubject = new BehaviorSubject<number>(0);
  centerId$ = this.centerIdSubject.asObservable();

  private userIdSubject = new BehaviorSubject<string | null>(null);
  userId$ = this.userIdSubject.asObservable();

  setCenterId(centerId: number): void {
    this.centerIdSubject.next(centerId);
  }

  setUserId(userId: string | null): void {
    this.userIdSubject.next(userId);
  }

  getCurrentCenterId(): number {
    return this.centerIdSubject.getValue();
  }

  getCurrentUserId(): string | null {
    return this.userIdSubject.getValue();
  }
}

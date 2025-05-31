import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  baseUrl = environment.baseUrl;
  private notificationsSubject = new BehaviorSubject<any[]>([]);
  public notifications$ = this.notificationsSubject.asObservable();

  constructor(private http: HttpClient) {}

  public loadInitialNotifications() {
    this.http.get<any[]>(this.baseUrl + 'notifications')
      .subscribe(notifs => this.notificationsSubject.next(notifs));
  }

  public markAsRead(notificationId: number) {
    return this.http.put(this.baseUrl + `notifications/${notificationId}/read`, {});
  }
}

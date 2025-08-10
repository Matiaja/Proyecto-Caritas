import { HttpClient } from '@angular/common/http';
import { Injectable, OnDestroy } from '@angular/core';
import { environment } from '../../../environments/environment';
import { BehaviorSubject, Observable } from 'rxjs';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { AuthService } from '../../auth/auth.service';

@Injectable({
  providedIn: 'root'
})
export class NotificationService implements OnDestroy {
  private hubConnection!: HubConnection;
  baseUrl = environment.baseUrl;
  notificationHubUrl = environment.notificationHubUrl;
  private notificationsSubject = new BehaviorSubject<any[]>([]);
  public notifications$ = this.notificationsSubject.asObservable();

  constructor(private http: HttpClient, private authService: AuthService) {}

  public initializeConnection() {
    const token = this.authService.getToken();
    if (!token) {
      console.error('No hay token disponible');
      return;
    }

    if (this.hubConnection?.state === 'Connected') return;

    this.buildConnection(token);
    this.registerHandlers();
    this.startConnection();
  }
  
  private buildConnection(token: string) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.notificationHubUrl, {
        accessTokenFactory: () => token
      })
      .configureLogging(LogLevel.None)
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .build();
  }

  private startConnection() {
    this.hubConnection.start()
      .then(() => {
        this.loadInitialNotifications();
      })
      .catch(err => {
        console.error('Error while starting connection: ', err);
      });
  }

  private registerHandlers() {
    this.hubConnection.on('ReceiveNotification', (notification: any) => {
      const currentNotifications = this.notificationsSubject.value;
      if (!currentNotifications.some(n => n.id === notification.id)) {
        this.notificationsSubject.next([notification, ...currentNotifications]);
      }
    });

    this.hubConnection.onreconnecting(() => {
    });

    this.hubConnection.onreconnected(() => {
    });
  }

  public loadInitialNotifications() {
    this.http.get<any[]>(this.baseUrl + 'notifications')
      .subscribe(notifs => this.notificationsSubject.next(notifs));
  }

  public removeNotification(notificationId: number) {
    const current = this.notificationsSubject.value;
    const updated = current.filter(n => n.id !== notificationId);
    this.notificationsSubject.next(updated);
  }

  public acceptAssignment(notification: any): Observable<any> {
    return this.http.post(`${this.baseUrl}notifications/accept`, {
      orderLineId: notification.orderLineId,
      donationRequestId: notification.donationRequestId,
      idNotification: notification.id
    });
  }

  markAsShipped(notification: any): Observable<any> {
    return this.http.post(`${this.baseUrl}notifications/ship`, {
      orderLineId: notification.orderLineId,
      donationRequestId: notification.donationRequestId,
      idNotification: notification.id
    });
  }

  confirmReceipt(notification: any): Observable<any> {
    return this.http.post(`${this.baseUrl}notifications/receive`, {
      orderLineId: notification.orderLineId,
      donationRequestId: notification.donationRequestId,
      idNotification: notification.id
    });
  }

  public markAsRead(notificationId: number) {
    return this.http.put(this.baseUrl + `notifications/${notificationId}/read`, {});
  }

  ngOnDestroy() {
    this.hubConnection?.stop();
  }
}

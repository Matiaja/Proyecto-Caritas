import { HttpClient } from '@angular/common/http';
import { Injectable, OnDestroy } from '@angular/core';
import { environment } from '../../../environments/environment';
import { BehaviorSubject, Observable } from 'rxjs';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { AuthService } from '../../auth/auth.service';
import { ToastrService } from 'ngx-toastr';

@Injectable({
  providedIn: 'root'
})
export class NotificationService implements OnDestroy {
  private hubConnection!: HubConnection;
  baseUrl = environment.baseUrl;
  notificationHubUrl = environment.notificationHubUrl;
  private notificationsSubject = new BehaviorSubject<any[]>([]);
  public notifications$ = this.notificationsSubject.asObservable();

  private currentToken: string | null = null; // <- trackea el token actual

  constructor(private http: HttpClient, private authService: AuthService, private toast: ToastrService) {}

  public initializeConnection() {
    const token = this.authService.getToken();

    // Si no hay token (logout), cerramos y limpiamos
    if (!token) {
      this.teardownConnection();
      return;
    }

    // Si el token cambió, hacemos teardown y reconstruimos
    if (this.currentToken !== token) {
      this.teardownConnection();
      this.currentToken = token;
      this.buildConnection(token);
      this.registerHandlers();
      this.startConnection();
      return;
    }

    // Si el token es el mismo pero la conexión no está viva, arrancamos
    if (!this.hubConnection || this.hubConnection.state !== 'Connected') {
      this.buildConnection(token);
      this.registerHandlers();
      this.startConnection();
    }
  }

  // Llamar esto explícitamente tras login/logout si preferís
  public reinitializeForCurrentUser() {
    this.initializeConnection();
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
        notification.hasOpenedOnce = false;
        this.notificationsSubject.next([notification, ...currentNotifications]);
        this.toast.success('Nueva notificación recibida');
      }
    });

    this.hubConnection.onreconnecting(() => { /* opcional */ });
    this.hubConnection.onreconnected(() => { /* opcional */ });
  }

  // Cierra conexión y limpia el estado local
  private teardownConnection() {
    try {
      if (this.hubConnection) {
        this.hubConnection.off('ReceiveNotification');
        this.hubConnection.stop().catch(() => {});
      }
    } catch {}
    // Limpiar lista (evita mostrar notificaciones del usuario anterior)
    this.notificationsSubject.next([]);
  }

  public loadInitialNotifications() {
    const url = `${this.baseUrl}notifications?_=${Date.now()}`; // cache-busting
    this.http.get<any[]>(url, {
      headers: {
        'Cache-Control': 'no-cache',
        'Pragma': 'no-cache',
        'Expires': '0'
      }
    }).subscribe(notifs => this.notificationsSubject.next(notifs));
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
    this.teardownConnection();
  }
}

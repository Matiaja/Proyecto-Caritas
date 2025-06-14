import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { NotificationService } from '../../../services/notification/notification.service';
import { NOTIFICATION_ACTIONS, NotificationType } from '../../../models/notification.model';
import { Subject, takeUntil } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-notification',
  standalone: true,
  imports: [
    CommonModule
  ],
  templateUrl: './notification.component.html',
  styleUrl: './notification.component.css'
})
export class NotificationComponent implements OnInit, OnDestroy {

  notifications: any[] = [];
  unreadCount = 0;
  private destroy$ = new Subject<void>();

  constructor(
    private notificationService: NotificationService,
    private toastrService: ToastrService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.notificationService.notifications$
      .pipe(takeUntil(this.destroy$))
      .subscribe(notifs => {
        this.notifications = notifs;
        this.unreadCount = notifs.filter((n: any) => !n.isRead).length;
    });

    this.notificationService.initializeConnection();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
  
  getActionsForType(type: NotificationType) {
    return NOTIFICATION_ACTIONS[type] || [];
  }

  handleAction(notification: Notification, action: string) {
    const actionHandlers: Record<string, () => void> = {
      'accept': () => this.acceptAssignment(notification),
      'reject': () => this.rejectAssignment(notification),
      'mark_as_shipped': () => this.markAsShipped(notification),
      // 'confirm_receipt': () => this.confirmReceipt(notification),
      // 'reassign': () => this.reassignNotification(notification)
    };

    const handler = actionHandlers[action];
    if (handler) handler();
  }

  acceptAssignment(notification: any) {
    const originalNotifications = [...this.notifications];
    
    notification.isRead = true; // Marcar como leída inmediatamente
    this.notificationService.acceptAssignment(notification).subscribe({
      next: () => {
        this.toastrService.success('Asignación aceptada correctamente');
      },
      error: (err) => {
        console.error('Error al aceptar:', err);
        this.notifications = originalNotifications;
        this.unreadCount = originalNotifications.filter((n: any) => !n.isRead).length;
        this.toastrService.error('Error al aceptar la asignación');
      }
    })
  }

  rejectAssignment(notification: any) {
    // Lógica para rechazar
    console.log('Rechazando asignación:', notification);
  }
  
  markAsShipped(notification: any) {
    // Lógica para enviar
    console.log('Enviando:', notification);
  }

  shouldShowActions(notification: any): boolean {
    // No mostrar acciones si la notificación está marcada como leída
    return !notification.isRead && this.getActionsForType(notification.type).length > 0;
  }

  trackByNotificationId(index: number, notification: any): string {
    return `${notification.id}-${notification.isRead}-${notification.type}`;
  }

  markAsRead() {
    this.notifications.forEach(notification => {
      if (!notification.isRead && notification.type == 'System') {
        this.notificationService.markAsRead(notification.id).subscribe(() => {
          notification.isRead = true;
          this.unreadCount--;
        });
      }
    });
  }	

  handleNotification(notification: any) {
    if (!notification.isRead) {
      this.notificationService.markAsRead(notification.id).subscribe(() => {
        notification.isRead = true;
        this.unreadCount--;
      });
    }
    // navegar al detalle de la orderLine o similar si querés
  }

}

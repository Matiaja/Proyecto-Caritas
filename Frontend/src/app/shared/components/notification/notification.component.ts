import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { NotificationService } from '../../../services/notification/notification.service';
import { NOTIFICATION_ACTIONS, NotificationType } from '../../../models/notification.model';
import { Subject, takeUntil } from 'rxjs';

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
  showDropdown = false;
  dropdownAnimation = '';
  private destroy$ = new Subject<void>();

  constructor(
    private notificationService: NotificationService,
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

  getActionsForType(type: NotificationType) {
    return NOTIFICATION_ACTIONS[type] || [];
  }

  toggleDropdown() {
    this.showDropdown = !this.showDropdown;
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  handleAction(notification: Notification, action: string) {
    const actionHandlers: Record<string, () => void> = {
      'accept': () => this.acceptAssignment(notification),
      'reject': () => this.rejectAssignment(notification),
      // 'mark_as_shipped': () => this.markAsShipped(notification),
      // 'confirm_receipt': () => this.confirmReceipt(notification),
      // 'reassign': () => this.reassignNotification(notification)
    };

    const handler = actionHandlers[action];
    if (handler) handler();
  }

  acceptAssignment(notification: any) {
    // Lógica para aceptar la asignación
    console.log('Aceptando asignación:', notification);
  }

  rejectAssignment(notification: any) {
    // Lógica para rechazar
    console.log('Rechazando asignación:', notification);
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

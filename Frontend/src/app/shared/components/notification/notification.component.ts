import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { NotificationService } from '../../../services/notification/notification.service';

@Component({
  selector: 'app-notification',
  standalone: true,
  imports: [
    CommonModule
  ],
  templateUrl: './notification.component.html',
  styleUrl: './notification.component.css'
})
export class NotificationComponent implements OnInit {

  notifications: any[] = [];
  unreadCount = 0;
  showDropdown = false;

  constructor(private notificationService: NotificationService) {}

  ngOnInit() {
    this.notificationService.notifications$.subscribe(notifs => {
      this.notifications = notifs;
      this.unreadCount = notifs.filter(n => !n.isRead).length;
    });

    // this.notificationService.startConnection(this.userId);
    this.notificationService.loadInitialNotifications();
  }

  toggleDropdown() {
    this.showDropdown = !this.showDropdown;
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

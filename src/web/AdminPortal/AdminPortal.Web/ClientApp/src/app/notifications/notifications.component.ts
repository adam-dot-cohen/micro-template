import { Component } from '@angular/core';
import { NotificationService } from './_services/notification.service';

@Component({
  selector: 'app-notifications',
  templateUrl: './notifications.component.html',
  styleUrls: ['./notifications.component.scss']
})

export class NotificationsComponent {
  public notificationsCount = 0; // TODO: Initialize from persistent notifications.

  constructor(private readonly notificationService: NotificationService) {

    //notificationService.onNotification(() => )
  }
}

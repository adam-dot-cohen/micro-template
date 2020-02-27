import { Component } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { NotificationService } from './_services/notification.service';

@Component({
  selector: 'app-notifications',
  templateUrl: './notifications.component.html',
  styleUrls: ['./notifications.component.scss']
})

export class NotificationsComponent {
  public notificationsCount = 0; // TODO: Initialize from persistent notifications.

  constructor(notificationService: NotificationService,
    private readonly _snackBar: MatSnackBar) {

    notificationService.onNotify = (data) => this.onNotify(data);
  }

  private onNotify(message: string) {
    this._snackBar.open(message, 'DISMISS', { duration: 5000 });

    this.notificationsCount++;
  }
}

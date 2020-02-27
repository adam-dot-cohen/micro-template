import { Injectable } from '@angular/core';
import { HubConnection } from '@microsoft/signalr';
import * as signalR from '@microsoft/signalr';

@Injectable({
  providedIn: 'root'
})

export class NotificationService {
  private readonly hubConnection: HubConnection | undefined;

  constructor() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:5001/notifications')
      .build();

    this.hubConnection.start().catch(err => console.error(err.toString()));

    this.hubConnection.on('Notification', (data: any) => {

    });
  }

  public onNotification()
  {

  }
}

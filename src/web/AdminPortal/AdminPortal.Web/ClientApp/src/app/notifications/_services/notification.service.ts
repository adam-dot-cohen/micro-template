import { Injectable } from '@angular/core';
import { HubConnection } from '@microsoft/signalr';
import * as signalR from '@microsoft/signalr';
import { environment } from '@env/environment';

@Injectable({
  providedIn: 'root'
})

export class NotificationService {
  private readonly hubConnection: HubConnection | undefined;

  constructor() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(environment.notificationsHubUrl)
      .build();

    this.hubConnection.start()
      .catch(err => console.error(err.toString()));

    this.hubConnection.on('Notify', (data: any) => this.onNotify(data));
  }

  public onNotify: (data) => void;
}

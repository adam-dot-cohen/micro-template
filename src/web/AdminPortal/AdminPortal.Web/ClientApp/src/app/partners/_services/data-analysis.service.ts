import { Injectable } from '@angular/core';
import { HubConnection } from '@microsoft/signalr';
import * as signalR from '@microsoft/signalr';
import { environment } from '@env/environment';

@Injectable({
  providedIn: 'root'
})
export class DataAnalysisService {

  private readonly hubConnection: HubConnection | undefined;

  constructor() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(environment.dataAnalysisHubUrl)
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start()
      .catch(err => console.error(err));

    this.hubConnection.on('Updated', (data: any) => this.onUpdated(data));
  }

  public onUpdated: (data) => void;
}

import { Component, HostBinding, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MatTableDataSource } from '@angular/material';

@Component({
  selector: 'app-fetch-data',
  templateUrl: './fetch-data.component.html',
  styleUrls: ['./fetch-data.component.scss']
})

export class FetchDataComponent {
  @HostBinding('class') public classes = 'app-main-content';

  public forecasts: MatTableDataSource<WeatherForecast>;
  public columnsToDisplay = [ 'date', 'temperatureC', 'temperatureF', 'summary' ];

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    http.get<WeatherForecast[]>(baseUrl + 'weatherforecast').subscribe(result => {
      this.forecasts = new MatTableDataSource(result);
    }, error => console.error(error));
  }
}

interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}

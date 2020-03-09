import { Component, HostBinding } from '@angular/core';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})

export class HomeComponent {
  @HostBinding('class') public classes = 'app-main-content';

  public isProduction = environment.production;
}

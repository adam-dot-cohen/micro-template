import { Component, HostBinding } from '@angular/core';

@Component({
  selector: 'app-theme-sample',
  templateUrl: './theme-sample.component.html'
})

export class ThemeSampleComponent {
  @HostBinding('class') public classes = 'app-main-content';
}

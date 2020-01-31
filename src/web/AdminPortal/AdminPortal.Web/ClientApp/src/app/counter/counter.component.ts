import { Component, HostBinding } from '@angular/core';

@Component({
  selector: 'app-counter-component',
  templateUrl: './counter.component.html',
  styleUrls: ['./counter.component.css']
})

export class CounterComponent {
  @HostBinding('class') public classes = 'app-main-content';

  public currentCount = 0;

  public incrementCounter() {
    this.currentCount++;
  }
}

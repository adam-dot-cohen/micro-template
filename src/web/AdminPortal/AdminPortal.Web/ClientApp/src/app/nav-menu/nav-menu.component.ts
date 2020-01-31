import { Component } from '@angular/core';
import { BreakpointObserver } from '@angular/cdk/layout';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.scss']
})

export class NavMenuComponent {

  constructor(private readonly breakpointObserver: BreakpointObserver) {
  }
}

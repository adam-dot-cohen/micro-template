import { Component, Renderer2 } from '@angular/core';
import { environment } from '@env/environment';
import { AuthorizeService } from '@app/api-auth/authorize.service';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.scss']
})

export class NavMenuComponent {
  public isProduction = environment.production;
  private currentTheme = 'laso-dark-theme';

  constructor(
    private readonly renderer: Renderer2,
    private readonly authorizeService: AuthorizeService) {
  }

  logout(): void {
    this.authorizeService.logout();
  }

  public toggleTheme() {
    // TODO: Some type of theme service that handles state and persistence.
    if (this.currentTheme === 'laso-dark-theme') {
      this.currentTheme = 'laso-light-theme';
    } else {
      this.currentTheme = 'laso-dark-theme';
    }

    this.renderer.setAttribute(document.body, 'class', this.currentTheme);
  }
}

import { Component, Renderer2, OnDestroy } from '@angular/core';
import { environment } from '@env/environment';
import { AuthorizeService } from '@app/api-auth/authorize.service';
import { Subscription } from 'rxjs';
import { filter, map } from 'rxjs/operators';
import { MediaChange, MediaObserver } from '@angular/flex-layout';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.scss']
})

export class NavMenuComponent implements OnDestroy {
  private currentTheme = 'laso-dark-theme';
  private readonly mediaWatcher: Subscription;

  public isProduction = environment.production;

  public opened: boolean;
  public small: boolean;
  public mode: string;

  constructor(
    private readonly renderer: Renderer2,
    private readonly authorizeService: AuthorizeService,
    public mediaObserver: MediaObserver) {

    this.mediaWatcher = mediaObserver.asObservable()
      .pipe(
        filter((changes: MediaChange[]) => changes.length > 0),
        map((changes: MediaChange[]) => changes[0])
      )
        .subscribe((change: MediaChange) => {
          this.opened = this.mediaObserver.isActive('gt-sm');
          this.small = this.opened && this.mediaObserver.isActive('md');
          this.mode = this.opened ? 'side' : 'over';
      });
  }

  public ngOnDestroy() {
    this.mediaWatcher.unsubscribe();
  }

  public closeDrawer(drawer) {
    if (this.mode !== 'side') {
      drawer.close();
    }
  }

  public logout(): void {
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

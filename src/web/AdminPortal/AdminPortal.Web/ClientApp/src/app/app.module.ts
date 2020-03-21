import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { LoadingBarRouterModule } from '@ngx-loading-bar/router';
import { LoadingBarHttpClientModule } from '@ngx-loading-bar/http-client';

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FlexLayoutModule } from '@angular/flex-layout';

import { MatBadgeModule } from '@angular/material/badge';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatMomentDateModule } from '@angular/material-moment-adapter';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatToolbarModule } from '@angular/material/toolbar';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { NotificationsComponent } from './notifications/notifications.component';
import { HomeComponent } from './home/home.component';

import { PartnersModule } from '@app/partners/partners.module';
import { ApiAuthModule } from '@app/api-auth/api-auth.module';
import { DemoModule } from '@app/demo/demo.module';
import { AuthorizeInterceptor } from './api-auth/authorize.interceptor';

@NgModule({
  declarations: [
    AppComponent,
    NotificationsComponent,
    NavMenuComponent,
    HomeComponent
  ],

  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    LoadingBarHttpClientModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' }
    ]),

    LoadingBarRouterModule,
    BrowserAnimationsModule,
    FlexLayoutModule,

    MatButtonModule,
    MatBadgeModule,
    MatCardModule,
    MatDividerModule,
    MatIconModule,
    MatListModule,
    MatMomentDateModule,
    MatSidenavModule,
    MatSnackBarModule,
    MatToolbarModule,

    PartnersModule,
    ApiAuthModule,
    DemoModule
  ],

  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: AuthorizeInterceptor, multi: true }
  ],

  bootstrap: [AppComponent]
})
export class AppModule {
}

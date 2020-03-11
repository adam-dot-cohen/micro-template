import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { LoadingBarRouterModule } from '@ngx-loading-bar/router';
import { LoadingBarHttpClientModule } from '@ngx-loading-bar/http-client';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { NotificationsComponent } from './notifications/notifications.component';
import { HomeComponent } from './home/home.component';

import { AppUiModule } from '@app/shared/app-ui.module';
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
    AppUiModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' }
    ]),
    LoadingBarRouterModule,

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

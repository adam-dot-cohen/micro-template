import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule  } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';

import { AppUiModule } from '@app/shared/app-ui.module';

import { PartnerResolver } from './_resolvers/partner.resolver';

import { PartnerListComponent } from './partner-list/partner-list.component';
import { PartnerDetailComponent } from './partner-detail/partner-detail.component';
import { PartnerConfigurationComponent } from './partner-configuration/partner-configuration.component';
import { CreatePartnerComponent } from './create-partner/create-partner.component';

@NgModule({
  imports: [
    HttpClientModule,
    FormsModule,

    RouterModule.forChild([
      { path: 'partners', component: PartnerListComponent },
      { path: 'partners/detail/:id', component: PartnerDetailComponent, resolve: { partner: PartnerResolver } },
      { path: 'partners/configuration/:id', component: PartnerConfigurationComponent },
      { path: 'partners/create', component: CreatePartnerComponent }
    ]),
    CommonModule,
    AppUiModule
  ],
  declarations: [
    PartnerListComponent,
    PartnerDetailComponent,
    CreatePartnerComponent,
    PartnerConfigurationComponent
  ]
})
export class PartnersModule { }

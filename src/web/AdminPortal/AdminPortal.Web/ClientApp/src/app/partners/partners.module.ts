import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule  } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';

import { AppUiModule } from "@app/shared/app-ui.module";

import { PartnerListComponent } from './partner-list/partner-list.component';
import { CreatePartnerComponent } from './create-partner/create-partner.component';

@NgModule({
  imports: [
    HttpClientModule,
    FormsModule,

    RouterModule.forChild([
      { path: 'partners', component: PartnerListComponent },
      { path: 'partners/create-partner', component: CreatePartnerComponent },
      { path: 'partners/edit-partner', component: CreatePartnerComponent  }
    ]),
    CommonModule,
    AppUiModule
  ],
  declarations: [
    PartnerListComponent,
    CreatePartnerComponent
  ]
})
export class PartnersModule { }

import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { PartnerListComponent } from './partner-list/partner-list.component';
import { CreatePartnerComponent } from './create-partner/create-partner.component';

@NgModule({
  imports: [
    RouterModule.forChild([
      { path: 'partners', component: PartnerListComponent },
      { path: 'create-partner', component: CreatePartnerComponent }
    ]),
    CommonModule
  ],
  declarations: [
    PartnerListComponent,
    CreatePartnerComponent
  ]
})
export class PartnersModule { }

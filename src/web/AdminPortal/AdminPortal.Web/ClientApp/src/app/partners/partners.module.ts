import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule  } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';

import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatTableModule } from '@angular/material/table';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatExpansionModule} from '@angular/material/expansion';

import { PartnerResolver } from './_resolvers/partner.resolver';
import { PartnerConfigurationResolver } from './_resolvers/partnerconfiguration.resolver';

import { PartnerListComponent } from './partner-list/partner-list.component';
import { PartnerDetailComponent } from './partner-detail/partner-detail.component';
import { PartnerConfigurationComponent } from './partner-configuration/partner-configuration.component';
import { CreatePartnerComponent } from './create-partner/create-partner.component';
import { PartnerAnalysisHistoryComponent } from './partner-analysis-history/partner-analysis-history.component';

@NgModule({
  imports: [
    HttpClientModule,
    FormsModule,

    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatInputModule,
    MatListModule,
    MatTableModule,
    MatToolbarModule,
    MatExpansionModule,

    RouterModule.forChild([
      {
         path: 'partners',
         component: PartnerListComponent
      },
      {
         path: 'partners/:id/detail',
         component: PartnerDetailComponent,
         resolve: { partner: PartnerResolver }
      },
      {
         path: 'partners/:id/configuration',
         component: PartnerConfigurationComponent,
         resolve: { configuration: PartnerConfigurationResolver }
      },
      {
         path: 'partners/:id/analysis-history',
         component: PartnerAnalysisHistoryComponent
      },
      {
         path: 'partners/create',
         component: CreatePartnerComponent
      }
    ]),
    CommonModule
  ],
  declarations: [
    PartnerListComponent,
    PartnerDetailComponent,
    CreatePartnerComponent,
    PartnerConfigurationComponent,
    PartnerAnalysisHistoryComponent
  ]
})
export class PartnersModule { }

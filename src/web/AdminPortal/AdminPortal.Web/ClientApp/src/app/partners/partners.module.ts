import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule  } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FlexLayoutModule } from '@angular/flex-layout';

import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatRadioModule } from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatBadgeModule } from '@angular/material/badge';
import { MatChipsModule } from '@angular/material/chips';
import { MatMomentDateModule } from '@angular/material-moment-adapter';
import { MatSliderModule } from '@angular/material/slider';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatListModule } from '@angular/material/list';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTableModule } from '@angular/material/table';

import { PartnerListComponent } from './partner-list/partner-list.component';
import { CreatePartnerComponent } from './create-partner/create-partner.component';

@NgModule({
  imports: [
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCheckboxModule,
    MatRadioModule,
    MatSelectModule,
    MatIconModule,
    MatBadgeModule,
    MatChipsModule,
    MatMomentDateModule,
    MatSliderModule,
    MatSlideToggleModule,
    MatListModule,
    MatToolbarModule,
    MatTableModule,

    HttpClientModule,
    FormsModule,

    RouterModule.forChild([
      { path: 'partners', component: PartnerListComponent },
      { path: 'partners/create-partner', component: CreatePartnerComponent },
      { path: 'partners/edit-partner', component: CreatePartnerComponent  }
    ]),
    CommonModule,
    FlexLayoutModule
  ],
  declarations: [
    PartnerListComponent,
    CreatePartnerComponent
  ]
})
export class PartnersModule { }

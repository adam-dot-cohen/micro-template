import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppUiModule } from '@app/shared/app-ui.module';

import { CounterComponent } from './counter/counter.component';
import { FetchDataComponent } from './fetch-data/fetch-data.component';
import { ThemeSampleComponent } from './theme-sample/theme-sample.component';

@NgModule({
  declarations: [
    CounterComponent,
    FetchDataComponent,
    ThemeSampleComponent
  ],
  imports: [
    CommonModule,
    AppUiModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forChild([
      { path: 'counter', component: CounterComponent },
      { path: 'fetch-data', component: FetchDataComponent },
      { path: 'theme-sample', component: ThemeSampleComponent }
    ])
  ]
})
export class DemoModule { }
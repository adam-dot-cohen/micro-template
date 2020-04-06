import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { PartnerConfiguration } from '@app/partners/_models/partnerconfiguration';

@Component({
  selector: 'app-partner-configuration',
  templateUrl: './partner-configuration.component.html',
  styleUrls: ['./partner-configuration.component.scss']
})
export class PartnerConfigurationComponent implements OnInit {

  public displayedColumns = ['category', 'name', 'value'];
  public configuration: PartnerConfiguration;

  constructor(private readonly route: ActivatedRoute) {
  }

  public ngOnInit() {
    this.configuration = this.route.snapshot.data['configuration'];
  }

  public showSettingValue(setting) {
    setting.isVisible = true;
  }

  public hideSettingValue(setting) {
    setting.isVisible = false;
  }
}

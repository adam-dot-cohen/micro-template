import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { PartnerProvisioningHistory } from '@app/partners/_models/partner-provisioning-history';

@Component({
  selector: 'app-partner-provisioning-history',
  templateUrl: './partner-provisioning-history.component.html',
  styleUrls: ['./partner-provisioning-history.component.scss']
})
export class PartnerProvisioningHistoryComponent implements OnInit {

  public displayedColumns = ['started','eventType','succeeded','completed','errors'];
  public provisioningHistory: PartnerProvisioningHistory;

  constructor(private readonly route: ActivatedRoute) { }

  ngOnInit(): void {
    this.provisioningHistory = this.route.snapshot.data['provisioningHistory'];
  }

}

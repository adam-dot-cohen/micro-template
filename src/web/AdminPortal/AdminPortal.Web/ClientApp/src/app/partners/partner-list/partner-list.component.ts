import { Component, OnInit } from '@angular/core';

import { PartnerService } from '@app/partners/_services/partner.service';
import { Partner } from '@app/partners/_models/partner';

@Component({
  templateUrl: './partner-list.component.html',
  styleUrls: ['./partner-list.component.scss']
})

export class PartnerListComponent implements OnInit {
  constructor(private readonly partnerService: PartnerService) { }

  public partners: Partner[] = [];

  public ngOnInit() {
    this.loadPartners();
  }

  private loadPartners() {
    this.partnerService.getPartners()
      .subscribe({
        next: result => this.partners = ((result) as any)
      });
  }
}

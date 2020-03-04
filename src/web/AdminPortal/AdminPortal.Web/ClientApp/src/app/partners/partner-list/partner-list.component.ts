import { Component, HostBinding, OnInit } from '@angular/core';
import { PartnerService } from '@app/partners/_services/partner.service';
import { Partner } from '@app/partners/_models/partner';

@Component({
  templateUrl: './partner-list.component.html',
  styleUrls: ['./partner-list.component.scss']
})

export class PartnerListComponent implements OnInit {
  public partners: Partner[] = [];
  public readonly displayedColumns: string[] = [
    'name',
    'contactName',
    'contactEmail',
    'contactPhone'
  ];

  constructor(private readonly partnerService: PartnerService) { }

  public ngOnInit() {
    this.partnerService.getPartners()
      .subscribe({
        next: result => this.partners = result
      });
  }
}

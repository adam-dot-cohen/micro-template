import { Component, OnInit, HostBinding } from '@angular/core';
import { Partner } from '../_models/partner';
import { Router } from '@angular/router';
import { PartnerService } from '../_services/partner.service';

@Component({
  templateUrl: './create-partner.component.html',
  styleUrls: ['./create-partner.component.scss']
})
export class CreatePartnerComponent implements OnInit {

  partner: Partner = new Partner();

  constructor(private partnerService: PartnerService, private router: Router) { }

  ngOnInit() {
  }

  onSave() {
    this.partnerService.createPartner(this.partner)
      .subscribe({
        // TODO: Show error message
        next: p => this.router.navigate(['partners'])
  });
  }

}

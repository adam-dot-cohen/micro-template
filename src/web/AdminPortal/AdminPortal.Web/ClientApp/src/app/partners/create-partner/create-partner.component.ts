import { Component, OnInit } from '@angular/core';
import { Partner } from '../_models/partner';
import { Router } from '@angular/router';
import { PartnerService } from '../_services/partner.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  templateUrl: './create-partner.component.html',
  styleUrls: ['./create-partner.component.scss']
})
export class CreatePartnerComponent implements OnInit {
  public partner = new Partner();

  constructor(
    private readonly partnerService: PartnerService,
    private readonly router: Router,
    private readonly snackBar: MatSnackBar) { }

  public ngOnInit() {
  }

  public onSave() {
    this.partnerService.createPartner(this.partner)
      .subscribe({
        // TODO: Show error message
        next: p => {
          this.snackBar.open(`Saved partner: ${p.name}`, 'dismiss', { duration: 5000 });
          this.router.navigate(['partners']);
        },
        error: e => {
          console.log(e);
          this.snackBar.open(`Error saving partner: ${e}`, 'dismiss');
        }
      });
  }
}

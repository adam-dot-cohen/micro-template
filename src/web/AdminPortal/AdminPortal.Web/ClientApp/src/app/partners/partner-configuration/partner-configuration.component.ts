import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { PartnerService } from '../_services/partner.service';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PartnerConfiguration } from '@app/partners/_models/partnerconfiguration';

@Component({
  selector: 'app-partner-configuration',
  templateUrl: './partner-configuration.component.html',
  styleUrls: ['./partner-configuration.component.scss']
})
export class PartnerConfigurationComponent implements OnInit {

  public displayedColumns = ['category', 'name', 'value'];
  public configuration: PartnerConfiguration;

  constructor(
    private readonly partnerService: PartnerService,
    private readonly route: ActivatedRoute,
    private readonly snackBar: MatSnackBar,
    private readonly router: Router
  ) {
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

  public onDelete() {
    this.partnerService.deletePartner(this.configuration.id)
      .subscribe({
        // TODO: Show error message
        next: () => {
          this.snackBar.open(`Deleted partner: ${this.configuration.name}`, 'dismiss', { duration: 5000 });
          this.router.navigate(['partners']);
        },
        error: e => {
          console.log(e);
          this.snackBar.open(`Error deleting partner: ${e}`, 'dismiss');
        }
      });
  }
}

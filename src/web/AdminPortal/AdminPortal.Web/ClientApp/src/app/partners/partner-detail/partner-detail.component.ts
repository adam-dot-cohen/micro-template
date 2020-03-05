import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Partner } from '@app/partners/_models/partner';

@Component({
  selector: 'app-partner-detail',
  templateUrl: './partner-detail.component.html',
  styleUrls: ['./partner-detail.component.scss']
})
export class PartnerDetailComponent implements OnInit {
  public partner: Partner;

  constructor(private readonly route: ActivatedRoute) {
  }

  public ngOnInit() {
    this.partner = this.route.snapshot.data['partner'];
  }
}

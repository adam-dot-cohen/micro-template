import { Component, OnInit, HostBinding } from '@angular/core';
import { Partner } from '../partner';

@Component({
  selector: 'app-create-partner',
  templateUrl: './create-partner.component.html',
  styleUrls: ['./create-partner.component.scss']
})
export class CreatePartnerComponent implements OnInit {
  // @HostBinding('class') public classes = 'app-main-content';

  partner: Partner;

  constructor() { }

  ngOnInit() {
  }

}

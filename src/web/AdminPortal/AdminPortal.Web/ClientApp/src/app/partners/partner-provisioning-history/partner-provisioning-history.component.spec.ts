import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';

import { PartnerProvisioningHistory } from '@app/partners/_models/partner-provisioning-history';
import { PartnerProvisioningHistoryComponent } from './partner-provisioning-history.component';

describe('PartnerProvisioningHistoryComponent', () => {
  let component: PartnerProvisioningHistoryComponent;
  let fixture: ComponentFixture<PartnerProvisioningHistoryComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PartnerProvisioningHistoryComponent ],
      imports: [
        FormsModule,
        MatTableModule,
        MatSnackBarModule,
        RouterTestingModule
        ],
      providers: [ { provide: ActivatedRoute, useClass: MockActivatedRoute } ],
      schemas: [ CUSTOM_ELEMENTS_SCHEMA ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PartnerProvisioningHistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

class MockActivatedRoute {
  public snapshot = { data: { 'provisioningHistory': new PartnerProvisioningHistory() } };
}

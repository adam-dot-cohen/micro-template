import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';

import { MatTableModule } from '@angular/material';
import { MatSnackBarModule } from '@angular/material';

import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';

import { PartnerConfiguration } from '@app/partners/_models/partnerconfiguration';
import { PartnerConfigurationComponent } from './partner-configuration.component';

describe('PartnerConfigurationComponent', () => {
  let component: PartnerConfigurationComponent;
  let fixture: ComponentFixture<PartnerConfigurationComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PartnerConfigurationComponent ],
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
    fixture = TestBed.createComponent(PartnerConfigurationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

class MockActivatedRoute {
  public snapshot = { data: { 'configuration': new PartnerConfiguration() } };
}

import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';

import { PartnerDetailComponent } from './partner-detail.component';
import { Partner } from '@app/partners/_models/partner';

describe('PartnerDetailComponent', () => {
  let component: PartnerDetailComponent;
  let fixture: ComponentFixture<PartnerDetailComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
        declarations: [ PartnerDetailComponent ],
        imports: [ RouterTestingModule ],
        providers: [ { provide: ActivatedRoute, useClass: MockActivatedRoute } ],
        schemas: [ CUSTOM_ELEMENTS_SCHEMA ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PartnerDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

class MockActivatedRoute {
  public snapshot = { data: { 'partner': new Partner() } };
}

import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';

import { MatSnackBarModule } from '@angular/material';

import { CreatePartnerComponent } from './create-partner.component';

describe('CreatePartnerComponent', () => {
  let component: CreatePartnerComponent;
  let fixture: ComponentFixture<CreatePartnerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CreatePartnerComponent ],
      imports: [
        FormsModule,
        HttpClientTestingModule,
        RouterTestingModule,

        MatSnackBarModule
      ],
      schemas: [ CUSTOM_ELEMENTS_SCHEMA ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CreatePartnerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

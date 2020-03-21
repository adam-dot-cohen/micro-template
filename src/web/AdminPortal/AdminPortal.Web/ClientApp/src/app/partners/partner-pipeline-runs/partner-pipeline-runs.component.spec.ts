import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PartnerPipelineRunsComponent } from './partner-pipeline-runs.component';

describe('PartnerPipelineRunsComponent', () => {
  let component: PartnerPipelineRunsComponent;
  let fixture: ComponentFixture<PartnerPipelineRunsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PartnerPipelineRunsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PartnerPipelineRunsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

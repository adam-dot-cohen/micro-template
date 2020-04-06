import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PartnerAnalysisHistoryComponent } from './partner-analysis-history.component';

describe('PartnerAnalysisHistoryComponent', () => {
  let component: PartnerAnalysisHistoryComponent;
  let fixture: ComponentFixture<PartnerAnalysisHistoryComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PartnerAnalysisHistoryComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PartnerAnalysisHistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

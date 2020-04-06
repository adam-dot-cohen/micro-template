import { TestBed } from '@angular/core/testing';

import { DataAnalysisService } from './data-analysis.service';

describe('DataAnalysisService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: DataAnalysisService = TestBed.get(DataAnalysisService);
    expect(service).toBeTruthy();
  });
});

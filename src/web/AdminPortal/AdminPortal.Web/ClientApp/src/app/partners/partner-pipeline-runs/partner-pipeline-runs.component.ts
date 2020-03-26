import { Component, OnInit } from '@angular/core';
import { PartnerService } from '../_services/partner.service';
import { PartnerAnalysisHistory } from '../_models/partner-pipeline-run';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-partner-pipeline-runs',
  templateUrl: './partner-pipeline-runs.component.html',
  styleUrls: ['./partner-pipeline-runs.component.scss']
})
export class PartnerPipelineRunsComponent implements OnInit {

  constructor(private readonly route: ActivatedRoute,
              private readonly partnerService: PartnerService) {
  }

  public analysisHistory: PartnerAnalysisHistory;
  public displayedColumns = ['timestamp', 'fileDataCategory', 'status'];
  public batchFilesDisplayedColumns = ['filename', 'dataCategory', 'contentLength'];
  public runStatusDisplayedColumns = ['timestamp', 'fileDataCategory', 'status'];
  public partnerId: string;

  public ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.partnerId = id;
      this.loadPipelineRuns(id);
    }
  }

  private loadPipelineRuns(partnerId: string) {
    this.partnerService.getPartnerPipelineRuns(partnerId)
      .subscribe({
        next: result => this.analysisHistory = result
      });
  }

  public refresh(): void {
    this.loadPipelineRuns(this.partnerId);
  }
}

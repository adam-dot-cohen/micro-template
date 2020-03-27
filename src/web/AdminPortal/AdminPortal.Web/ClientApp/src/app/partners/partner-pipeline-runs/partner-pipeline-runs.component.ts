import { Component, OnInit, OnDestroy } from '@angular/core';
import { PartnerService } from '../_services/partner.service';
import { PartnerAnalysisHistory, ProductAnalysisRun, FileBatch } from '../_models/partner-pipeline-run';
import { ActivatedRoute } from '@angular/router';
import { DataAnalysisService } from '../_services/data-analysis.service';

@Component({
  selector: 'app-partner-pipeline-runs',
  templateUrl: './partner-pipeline-runs.component.html',
  styleUrls: ['./partner-pipeline-runs.component.scss']
})
export class PartnerPipelineRunsComponent implements OnInit, OnDestroy {

  constructor(private readonly route: ActivatedRoute,
              private readonly partnerService: PartnerService,
              private readonly dataAnalysisService: DataAnalysisService) {
  }

  public batchFilesDisplayedColumns = ['filename', 'dataCategory', 'contentLength'];
  public runStatusDisplayedColumns = ['timestamp', 'dataCategory', 'status'];

  public analysisHistory: PartnerAnalysisHistory;
  public partnerId: string;

  public trackByRunId = (index: number, run: ProductAnalysisRun) => run.pipelineRunId;
  public trackByBatchId = (index: number, batch: FileBatch) => batch.fileBatchId;

  public ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.partnerId = id;
      this.loadAnalysisHistory(id);
      this.dataAnalysisService.onUpdated = (data) => this.refresh();
    }
  }

  public ngOnDestroy() {
    this.dataAnalysisService.onUpdated = (data: any) => {};
  }

  private loadAnalysisHistory(partnerId: string) {
    this.partnerService.getPartnerAnalysisHistory(partnerId)
      .subscribe({
        next: result => this.analysisHistory = result
      });
  }

  public refresh(): void {
    this.loadAnalysisHistory(this.partnerId);
  }
}

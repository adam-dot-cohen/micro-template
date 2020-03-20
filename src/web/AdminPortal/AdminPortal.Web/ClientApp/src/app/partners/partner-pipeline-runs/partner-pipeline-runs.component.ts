import { Component, OnInit } from '@angular/core';
import { PartnerService } from '../_services/partner.service';
import { PartnerPipelineRuns } from '../_models/partner-pipeline-run';
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

  public partnerRuns: PartnerPipelineRuns;
  public displayedColumns = ['timestamp', 'fileDataCategory', 'status'];
  private partnerId: string;

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
        next: result => this.partnerRuns = result
      });
  }

  public refresh(): void {
    this.loadPipelineRuns(this.partnerId);
  }
}

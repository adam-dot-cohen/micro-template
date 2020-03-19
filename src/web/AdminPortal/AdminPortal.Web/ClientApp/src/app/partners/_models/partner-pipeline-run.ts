export class PartnerPipelineRuns {
    public partnerId: string;
    public partnerName: string;
    public pipelineRuns: PipelineRun[];
}

export class PipelineRun {
    public runId: string;
    public  statuses: PipelineRunStatus[];
}

export class PipelineRunStatus {
    public status: string;
    public Timestamp: Date;
}

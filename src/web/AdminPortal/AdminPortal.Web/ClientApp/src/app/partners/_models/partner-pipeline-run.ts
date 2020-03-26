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
    public timestamp: Date;
    public fileDataCategory: string;
    public status: string;
}

export interface PartnerAnalysisHistory {
    partnerId: string;
    partnerName: string;
    fileBatches: FileBatch[];
}

export interface FileBatch {
    fileBatchId: string;
    status: string;
    created: Date;
    updated: Date;
    files: BlobFile[];
    productAnalysisRuns: ProductAnalysisRun[];
}

export interface ProductAnalysisRun {
    productName: string;
    pipelineRunId: string;
    requested: Date;
    statuses: StatusEvent[];
}

export interface BlobFile {
    id: string;
    filename: string;
    contentLength: number;
    dataCategory: string;
}

export interface StatusEvent {
    timestamp: Date;
    status: string;
    fileDataCategory: string;
}


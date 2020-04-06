export class PartnerAnalysisHistory {
    public partnerId: string;
    public partnerName: string;
    public fileBatches: FileBatch[];
}

export class FileBatch {
    public fileBatchId: string;
    public status: string;
    public created: Date;
    public updated: Date;
    public files: BlobFile[];
    public productAnalysisRuns: ProductAnalysisRun[];
}

export class ProductAnalysisRun {
    public productName: string;
    public pipelineRunId: string;
    public requested: Date;
    public statuses: StatusEvent[];
}

export class BlobFile {
    public id: string;
    public filename: string;
    public contentLength: number;
    public dataCategory: string;
}

export class StatusEvent {
    public correlationId: string;
    public timestamp: Date;
    public status: string;
    public dataCategory: string;
}


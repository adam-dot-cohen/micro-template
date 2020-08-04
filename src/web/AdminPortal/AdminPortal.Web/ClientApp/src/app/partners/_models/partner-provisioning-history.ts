export class PartnerProvisioningHistory {
  public id: string;
  public name: string;
  public history: Array<PartnerProvisioningEvent>;
}

export class PartnerProvisioningEvent {
  public eventType: string;
  public sequence: number;
  public started: string;
  public completed: string;
  public succeeded: boolean;
  public errors: string;
}

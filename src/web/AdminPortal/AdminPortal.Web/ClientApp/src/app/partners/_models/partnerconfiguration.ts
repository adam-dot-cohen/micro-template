export class PartnerConfiguration {
  public id: string;
  public name: string;
  public showDelete: boolean;
  public canDelete: boolean;
  public settings: Array<PartnerConfigurationSetting>;
}

export class PartnerConfigurationSetting {
  public category: string;
  public isSensitive: boolean;
  public name: string;
  public value: string;
}

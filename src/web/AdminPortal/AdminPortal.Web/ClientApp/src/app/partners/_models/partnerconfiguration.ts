export class PartnerConfiguration {
  public id: string;
  public name: string;
  public settings: Array<PartnerConfigurationSetting>;
}

export class PartnerConfigurationSetting {
  public category: string;
  public isSensitive: boolean;
  public name: string;
  public value: string;
}

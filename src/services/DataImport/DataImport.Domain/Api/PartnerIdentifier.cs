namespace DataImport.Domain.Api.Common
{
    // [Ed S] will probably want some higher level concept than an enum
    // once we have some use cases. Right now this just provides something
    // for factories to use.
    public enum PartnerIdentifier
    {
        Quarterspot,
        SterlingInternational,
        SonaBank,
        Laso
    }
}

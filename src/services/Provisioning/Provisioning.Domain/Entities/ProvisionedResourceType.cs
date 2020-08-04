namespace Provisioning.Domain.Entities
{
    //TODO: this should be converted into a collection of value objects maintained in the service (out of code)
    public enum ProvisionedResourceType
    {
        EscrowStorage,
        EscrowIncoming,
        EscrowOutgoing,
        ColdStorage,
        RawFileSystemDirectory,
        SFTPUsername,
        SFTPPassword,
        SFTPAccount,
        LasoPGPPublicKey,
        LasoPGPPrivateKey,
        LasoPGPPassphrase,
        PartnerPGPPublicKey,
        CuratedFileSystemDirectory,
        RejectedFileSystemDirectory,
        PublishedFileSystemDirectory,
        ExperimentFileSystemDirectory,
    }
}

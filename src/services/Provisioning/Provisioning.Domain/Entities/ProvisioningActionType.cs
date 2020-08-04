namespace Provisioning.Domain.Entities
{
    //TODO: this should be converted into a collection of value objects maintained in the service (out of code)
    public enum ProvisioningActionType
    {
        EscrowStorageProvisioned, 
        PGPKeysetProvisioned,
        FTPCredentialsProvisioned,
        ColdStorageProvisioned,
        FTPAccountProvisioned,
        DataProcessingDirectoriesProvisioned,
        ADGroupProvisioned,
        ADPermissionsAssigned,
        PartnerPublicKeyTestFileGenerated,
        LasoPartnerKeyTestFileGenerated,
        EscrowStorageRemoved,
        PGPKeysetRemoved,
        FTPCredentialsRemoved,
        ColdStorageRemoved,
        FTPAccountRemoved,
        DataProcessingDirectoriesRemoved,
        ADGroupRemoved,
        ADPermissionsRemoved
    }
}
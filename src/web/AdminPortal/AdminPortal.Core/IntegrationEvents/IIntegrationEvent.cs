namespace Laso.AdminPortal.Core.IntegrationEvents
{
    public interface IIntegrationEvent { }

    public interface IEnvelopedIntegrationEvent : IIntegrationEvent
    {
        (string Name, object Value) Discriminator { get; }
    }
}
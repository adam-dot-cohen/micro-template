using System.Threading.Tasks;

namespace Laso.IntegrationMessages
{
    public interface IMessageSender
    {
        Task Send<T>(T message) where T : IIntegrationMessage;
    }
}

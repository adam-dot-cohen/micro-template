using Grpc.Core;
using Laso.Identity.Core.Extensions;
using Laso.Identity.Core.Mediator;

namespace Laso.Identity.Infrastructure.Extensions
{
    public static class EnsureSuccessExtensions
    {
        public static TResponse ThrowRpcIfFailed<TResponse>(this TResponse response)
            where TResponse : Response
        {
            if (response.Success())
            {
                return response;
            }

            if (response.Exception != null)
            {
                throw new RpcException(new Status(StatusCode.Internal, response.Exception.InnermostException().Message));
            }

            var metadata = new Metadata();
            foreach (var message in response.ValidationMessages)
            {
                metadata.Add(message.Key, message.Message);
            }
            throw new RpcException(
                new Status(StatusCode.FailedPrecondition, response.GetAllMessages()),
                metadata);
        }
    }
}
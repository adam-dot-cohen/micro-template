using MediatR;

namespace Infrastructure.Mediation.Stream
{
    public interface IStream<TResult> : IRequest<StreamResponse<TResult>>
    {
    }
}
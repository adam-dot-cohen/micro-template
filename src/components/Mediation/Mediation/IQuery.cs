using MediatR;

namespace Laso.Mediation
{
    public interface IQuery<TResult> : IRequest<Response<TResult>>
    {
    }
}
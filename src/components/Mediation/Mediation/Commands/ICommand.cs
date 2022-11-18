using MediatR;

namespace Infrastructure.Mediation.Command
{
    public interface ICommand<TResult> : IRequest<CommandResponse<TResult>> { }

    public interface ICommand : IRequest<CommandResponse> { }
}
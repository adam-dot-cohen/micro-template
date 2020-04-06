namespace Laso.AdminPortal.Core.Mediator
{
    public interface ICommand : IMessage
    {
    }

    public interface ICommand<TResult> : IMessage
    {
    }
}

# Usage

## Messages

### Queries

Implement this interface:

```
public interface IQuery<TResult> : IRequest<QueryResponse<TResult>>
{
}
```

### Commands

Implement one of these `ICommand` interfaces depending upon whether the command returns a result:

```
public interface ICommand<TResult> : IRequest<CommandResponse<TResult>>
{
}

public interface ICommand : IRequest<CommandResponse>
{
}
```

### Validation

A message may implement `IInputValidator` to specify validation rules for the contents of the request message.

## Handlers

Extend the `abstract` base classes `QueryHandler` or `CommandHandler`, implement the `Handle` method in the derived message handler, and return a successful or failed `QueryResponse` or `CommandResponse` respectfully.  These base classes provide `Succeeded` and `Failed` helper methods (with overloads) for returning a constructed and correctly-typed response object.

If it is preferred to not use these convenience base classes, the `IQueryHandler` and `ICommandHandler` interfaces may be implemented directly and the handler responses can be generated using the factory methods on the `QueryResponse` and `CommandResponse` types.

## Mediator

The [MediatR](https://github.com/jbogard/MediatR) library is used to perform all mediator operations and extensibility points.  Please refer to that project's wiki for further details.

### Pipeline

MediatR supports [Behaviors](https://github.com/jbogard/MediatR/wiki/Behaviors) which can be used to create an execution pipeline for handling messages.  The default behaviors currently include:

- PerfLoggingPipelineBehavior: Logs handler execution time
- ErrorLoggingPipelineBehavior: Logs handler failures
- ExceptionPipelineBehavior: Catches a thrown exception and assigns it to the `Exception` property of the `QueryResponse` or `CommandResponse`
- ValidationPipelineBehavior: Executes validation if the message implements `IInputValidator`

# Setup

## Class Library Projects

Add a project reference to the `Mediation` project and begin using the types described above.

## Application Projects

This library can support any IoC container, but currently only [Lamar](https://jasperfx.github.io/lamar/documentation/ioc/) support is provided.  Any application using this mediation library must setup their application to use Lamar for dependency injection.

1. Add a project reference to the `Mediation.Configuration.Lamar` project.
2. Configure the Lamar IoC container `ServiceRegistry` to both scan for implemented handlers and register `IMediator` and the default 'behaviors' that compose the execution pipeline for handling messages. For example, this is taken from the AdminPortal:
```
private static void Initialize(ServiceRegistry x)
{
    x.Scan(scan =>
    {
        scan.Assembly("Laso.AdminPortal.Infrastructure");
        scan.WithDefaultConventions();

        scan.AddMediatorHandlers(); // <= Register handlers
    });

    x.AddMediator().WithDefaultMediatorBehaviors(); // <= Register mediator with default behaviors
    ...
}
```
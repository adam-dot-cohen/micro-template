using Lamar;
using Laso.Mediation.Configuration.Lamar;
using Microsoft.Extensions.Logging;

namespace Laso.Mediation.UnitTests.Configuration
{
    public class TestContainer : Container
    {
        public TestContainer(Repository repository) : base(x =>
        {
            x.For(typeof(ILogger<>)).Use(typeof(InMemoryLogger<>));
            x.For<Repository>().Use(repository);

            x.Scan(scan =>
            {
                scan.AssemblyContainingType<Repository>();
                scan.WithDefaultConventions();
                scan.AddMediatorHandlers();
            });

            x.AddMediator().WithDefaultMediatorBehaviors();
        }) { }
    }
}
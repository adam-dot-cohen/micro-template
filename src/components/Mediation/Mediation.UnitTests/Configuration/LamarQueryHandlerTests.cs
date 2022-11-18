using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Mediation.Query;
using MediatR;
using Shouldly;
using Xunit;

namespace Infrastructure.Mediation.UnitTests.Configuration
{
    public class LamarQueryHandlerTests
    {
        [Fact]
        public async Task Should_register_query_handlers()
        {
            var repository = new Repository();
            var container = new TestContainer(repository);

            var instance = container.GetInstance<IRequestHandler<TestQuery, QueryResponse<TestResult>>>();

            instance.ShouldBeOfType<TestHandler>();

            var query = new TestQuery();
            var result = new TestResult();
            repository.Add(typeof(TestResult), result);

            var response = await container.GetInstance<IMediator>().Send(query);

            repository[typeof(TestHandler)].ShouldBeSameAs(query);
            response.Result.ShouldBeSameAs(result);
        }

        public class TestResult { }
        public class TestQuery : IQuery<TestResult> { }

        public class TestHandler : QueryHandler<TestQuery, TestResult>
        {
            private readonly Repository _repository;

            public TestHandler(Repository repository)
            {
                _repository = repository;
            }

            public override Task<QueryResponse<TestResult>> Handle(TestQuery request, CancellationToken cancellationToken)
            {
                _repository.Add(typeof(TestHandler), request);

                return Task.FromResult(Succeeded((TestResult) _repository[typeof(TestResult)]));
            }
        }
    }
}
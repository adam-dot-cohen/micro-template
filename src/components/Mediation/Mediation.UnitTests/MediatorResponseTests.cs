using System;
using System.Linq;
using Infrastructure.Mediation;
using Infrastructure.Mediation.Command;
using Infrastructure.Mediation.Event;
using Infrastructure.Mediation.Query;
using Infrastructure.Mediation.Validation;
using Shouldly;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Infrastructure.Mediation.UnitTests
{
    public class MediatorResponseTests
    {
        [Fact]
        public void When_command_fails_Should_create_expected_response()
        {
            const string expectedMessage = "validation message";
            const string expectedKey = "key";

            var response = CommandResponse.Failed<TestResult>(expectedMessage);
            AssertFailureResponse(response, "", expectedMessage, false);

            response = CommandResponse.Failed<TestResult>(expectedKey, expectedMessage);
            AssertFailureResponse(response, expectedKey, expectedMessage, false);

            response = CommandResponse.Failed<TestResult>(new ValidationMessage(expectedKey, expectedMessage));
            AssertFailureResponse(response, expectedKey, expectedMessage, false);

            response = CommandResponse.Failed<TestResult>(new[] { new ValidationMessage(expectedKey, expectedMessage) });
            AssertFailureResponse(response, expectedKey, expectedMessage, false);

            response = CommandResponse.Failed<TestResult>(new Exception());
            AssertFailureResponse(response, null, null, true);

            response = new CommandResponse<TestResult>(new [] { new ValidationMessage(expectedKey, expectedMessage) }, new Exception());
            AssertFailureResponse(response, expectedKey, expectedMessage, true);

            response = CommandResponse.Failed<TestResult>(response);
            AssertFailureResponse(response, expectedKey, expectedMessage, true);
        }

        [Fact]
        public void When_command_succeeds_Should_create_expected_response()
        {
            var response = CommandResponse.Succeeded(new TestResult());
            response.Success.ShouldBeTrue();
            response.Result.ShouldNotBeNull();

            // cannot fail from succeeded message
            Assert.Throws<Exception>(() => CommandResponse.Failed<TestResult>(response));
        }

        [Fact]
        public void When_event_fails_Should_create_expected_response()
        {
            const string expectedMessage = "validation message";
            const string expectedKey = "key";

            var response = EventResponse.Failed(expectedMessage);
            AssertFailureResponse(response, "", expectedMessage, false);

            response = EventResponse.Failed(expectedKey, expectedMessage);
            AssertFailureResponse(response, expectedKey, expectedMessage, false);

            response = EventResponse.Failed(new ValidationMessage(expectedKey, expectedMessage));
            AssertFailureResponse(response, expectedKey, expectedMessage, false);

            response = EventResponse.Failed(new[] { new ValidationMessage(expectedKey, expectedMessage) });
            AssertFailureResponse(response, expectedKey, expectedMessage, false);

            response = EventResponse.Failed(new Exception());
            AssertFailureResponse(response, null, null, true);

            response = new EventResponse(new [] { new ValidationMessage(expectedKey, expectedMessage) }, new Exception());
            AssertFailureResponse(response, expectedKey, expectedMessage, true);

            response = EventResponse.Failed(response);
            AssertFailureResponse(response, expectedKey, expectedMessage, true);
        }

        [Fact]
        public void When_event_succeeds_Should_create_expected_response()
        {
            var response = EventResponse.Succeeded();
            response.Success.ShouldBeTrue();

            // cannot fail from succeeded message
            Assert.Throws<Exception>(() => EventResponse.Failed(response));
        }

        [Fact]
        public void When_query_fails_Should_create_expected_response()
        {
            const string expectedMessage = "validation message";
            const string expectedKey = "key";

            var response = QueryResponse.Failed<TestResult>(expectedMessage);
            AssertFailureResponse(response, "", expectedMessage, false);

            response = QueryResponse.Failed<TestResult>(expectedKey, expectedMessage);
            AssertFailureResponse(response, expectedKey, expectedMessage, false);

            response = QueryResponse.Failed<TestResult>(new ValidationMessage(expectedKey, expectedMessage));
            AssertFailureResponse(response, expectedKey, expectedMessage, false);

            response = QueryResponse.Failed<TestResult>(new[] { new ValidationMessage(expectedKey, expectedMessage) });
            AssertFailureResponse(response, expectedKey, expectedMessage, false);

            response = QueryResponse.Failed<TestResult>(new Exception());
            AssertFailureResponse(response, null, null, true);

            response = new QueryResponse<TestResult>(new [] { new ValidationMessage(expectedKey, expectedMessage) }, new Exception());
            AssertFailureResponse(response, expectedKey, expectedMessage, true);

            response = QueryResponse.Failed<TestResult>(response);
            AssertFailureResponse(response, expectedKey, expectedMessage, true);
        }

        [Fact]
        public void When_query_succeeds_Should_create_expected_response()
        {
            var response = QueryResponse.Succeeded(new TestResult());
            response.Success.ShouldBeTrue();
            response.Result.ShouldNotBeNull();

            // cannot fail from succeeded message
            Assert.Throws<Exception>(() => QueryResponse.Failed<TestResult>(response));
        }

        private static void AssertFailureResponse(Response response, string key, string message, bool hasException)
        {
            response.Success.ShouldBeFalse();
            if (key != null) response.ValidationMessages.Single().Key.ShouldBe(key);
            if (message != null) response.ValidationMessages.Single().Message.ShouldBe(message);
            (response.Exception != null).ShouldBe(hasException);
        }
    }
}

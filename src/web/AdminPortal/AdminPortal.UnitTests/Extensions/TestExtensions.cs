using Infrastructure.Mediation;
using Shouldly;

namespace Laso.AdminPortal.UnitTests.Extensions
{
    internal static class TestExtensions
    {
        public static T ShouldSucceed<T>(this T response)
            where T: Response
        {
            response.Success.ShouldBeTrue(response.GetAllMessages());
            return response;
        }

        public static T ShouldFail<T>(this T response)
            where T: Response
        {
            response.Success.ShouldBeFalse();
            return response;
        }
    }
}
using Atata;
using Insights.UITests.TestData.Identity.Users;
using Insights.UITests.UIComponents.AdminPortal.Pages.Login;
using NUnit.Framework;

namespace Insights.UITests.Tests.Identity.Login
{
    [TestFixture()]
    [Parallelizable(ParallelScope.Fixtures)]
    [Category("Smoke"), Category("PartnersTable")]
    public class LoginTest : TestFixtureBase
    {

        [Test]
        public void SignIn()
        {
            InsightsManagerUser insightsManagerUser = new InsightsManagerUser{Username = "ollie@laso.com",Password = "ollie"};
            Go.To<LoginPage>().SetForm(insightsManagerUser)
                ._SaveForm();

            Go.To<LoginPage>()
                .UserLink.Attributes.TextContent.Should.EqualIgnoringCase(insightsManagerUser.Username);
  
        }

    }
}

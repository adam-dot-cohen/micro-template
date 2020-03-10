using System;
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
            AdminUser adminUser = new AdminUser();
            Go.To<LoginPage>().SetForm(adminUser)
                ._SaveForm();

           
            Go.To<LoginPage>()
                .UserLink.
                Should.Exist().Attributes.TextContent.Should
                .EqualIgnoringCase(adminUser.Username);

        }
    }
}

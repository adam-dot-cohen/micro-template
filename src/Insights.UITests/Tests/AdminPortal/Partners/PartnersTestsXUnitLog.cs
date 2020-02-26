using Atata;
using Insights.UITests.UIComponents.AdminPortal.Controls.Pages.Partners;
using Xunit;
using Xunit.Abstractions;

namespace Insights.UITests.Tests.AdminPortal.Partners
{

    public class PartnersTestsXUnitLog : UITestFixture
    {
        /// <summary>
        /// It is required to define constructor with argument of ITestOutputHelper type for Xunit log output.
        /// </summary>
        /// <param name="output">The output.</param>
        public PartnersTestsXUnitLog(ITestOutputHelper output)
            : base(output)
        {
        }


        /// <summary>
        /// Use such approach with <see cref="UITestFixture.Execute(System.Action)"/> method when you need to add exception/error information to the log.
        /// It is needed if you log to file or other external source.
        /// It is not required when you just use ITestOutputHelper as a single log target.
        /// </summary>

        [Fact]
        public void CreatePartnerAllRequiredData()
        {
            Execute(() =>
            {
                Go.To<PartnerPage>().PrimaryContactEmail.Set("asds@thisemail.com")
                    .PartnerName.Set("financial institution")
                    .PrimaryContactName.Set("ollie arie")
                    .Save.Click();
            });
            

        }


    
    


    }
}

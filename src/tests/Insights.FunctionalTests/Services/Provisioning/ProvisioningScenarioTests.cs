using System.Threading.Tasks;
using Laso.Insights.FunctionalTests.Services.Identity;
using Xunit;

namespace Laso.Insights.FunctionalTests.Services.Provisioning
{
    [Trait("Capability", "Storage")]    // NOTE: Currently, this is required via configuration.
    public class ProvisioningScenarioTests
    {
        [Fact(Timeout = 2 * 60000)]
        public async Task Should_Provision_New_Partner()
        {
            var provisioning = new ProvisioningScenario();
            var identity = new IdentityScenario();

            try
            {
                await identity.CreatePartner();
            
                await Task.Delay(10000);
            }
            finally
            {
                await identity.DeletePartner();
            }
        }
    }
}

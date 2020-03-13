using Atata;
using Insights.UITests.TestData.Identity.Users;

namespace Insights.UITests.UIComponents.AdminPortal.Pages.Login 
{
    using _ =LoginPage;

    [VerifyTitle("IdentityServer4")]
    public class LoginPage : Page<LoginPage> 
    {
        
        public TextInput<_> Username { get; private set; }

        public PasswordInput<_> Password { get; private set; }

        public Button<_> Login { get; private set; }
        
        [FindByClass("dropdown-toggle")]
        public Link<_> UserLink { get; private set; }

        public Link<_> LogoutByName { get; private set; }

        public Link<_> LinkUserDynamicLocator(string user)
        {
            return Controls.CreateLink<_>("User logged in link "+user, new FindByXPathAttribute("//a[text()='"+user +" ']"));
        }


        public _ SetForm(InsightsManagerUser adminUser)
        {
            return
                Go.To<_>()
                    .Username.Set(adminUser.Username)
                    .Password.Set(adminUser.Password);


        }

        public _ _SaveForm()
        {
            return
            Login.Click();
        }

    }
}
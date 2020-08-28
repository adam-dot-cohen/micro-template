using Atata;
using Insights.UITests.TestData.Partners;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace Insights.UITests.UIComponents.AdminPortal.Pages.Partners
{
    using _ = PartnersConfigurationPage;

    [WaitForElement(WaitBy.XPath, "//mat-cell", Until.Visible, TriggerEvents.Init)]
    public class PartnersConfigurationPage : Page<PartnersConfigurationPage>
    {


        [FindByXPath("//*[contains(@class,'page-sub-header')]//mat-toolbar-row")]
        public Control<_> PageHeader { get; private set; }


        [ControlDefinition("mat-table", ContainingClass = "mat-table",
            ComponentTypeName = "mat-table")]
        public Table<ConfigurationTableHeader, ConfigurationTableRow, _> ConfigurationTable { get; private set; }


        [ControlDefinition("mat-header-cell", ComponentTypeName = "ConfigurationTable Table Header")]
        public class ConfigurationTableHeader : TableHeader<_>
        {
            public Control<_> Category { get; private set; }

            public Control<_> Name { get; private set; }
            public Control<_> Value { get; private set; }

        }


        [ControlDefinition("mat-row", ComponentTypeName = "ConfigurationTable Row")]
        public class ConfigurationTableRow : TableRow<_>
        {
            [FindByXPath(".//mat-cell[contains(@class,'category')]")]
            public Control<_> Category { get; private set; }

            [FindByXPath(".//mat-cell[contains(@class,'name')]")]
            public Control<_> Name { get; private set; }

            [FindByXPath(".//mat-cell[contains(@class,'value')]")]
            public Control<_> Value { get; private set; }

        }

        public List<Configuration> ConfigurationOnPartnerConfigurationPage()
        {
            List<Configuration> configurationList = new List<Configuration>();
            foreach (var row in ConfigurationTable.Rows)
            {
                string configurationValueString="";
                try
                {
                    IWebElement button = row.Value.Scope.FindElement(By.XPath(".//button"));
                    Actions actions = new Actions(driver:AtataContext.Current.Driver);
                    actions.ClickAndHold(button).Perform();
                     configurationValueString = row.Value.Scope.FindElement(By.XPath(".//mat-cell[contains(@class,'value')]/span[1]")).Text;

                }
                catch
                {
                    configurationValueString = row.Value.Content.Value;
                }

                configurationList.Add(new Configuration
                    {
                        Category = row.Category.Content.Value,
                        Name = row.Name.Content.Value,
                        Value = configurationValueString
                    });

            }
            return configurationList;
        }

    }
}




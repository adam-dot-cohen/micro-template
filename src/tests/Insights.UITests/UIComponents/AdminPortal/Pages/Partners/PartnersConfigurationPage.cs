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
        public Table<ProductTableHeader, ProductTableRow, _> ConfigurationTable { get; private set; }


        [ControlDefinition("mat-header-cell", ComponentTypeName = "ConfigurationTable Table Header")]
        public class ProductTableHeader : TableHeader<_>
        {
            public Control<_> Category { get; private set; }

            public Control<_> Name { get; private set; }
            public Control<_> Value { get; private set; }

        }


        [ControlDefinition("mat-row", ComponentTypeName = "ConfigurationTable Row")]
        public class ProductTableRow : TableRow<_>
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
            ConfigurationTable.Rows.Count.Should.Equal(6);
            List<Configuration> configurationList = new List<Configuration>();
            foreach (var row in ConfigurationTable.Rows)
            {
                string revealText="";
                try
                {
                    IWebElement button = row.Value.Scope.FindElement(By.XPath(".//button"));
                    Actions actions = new Actions(driver:AtataContext.Current.Driver);
                    actions.ClickAndHold(button).Perform();
                     revealText = row.Value.Scope.FindElement(By.XPath(".//mat-cell[contains(@class,'value')]/span[1]")).Text;

                }
                catch
                {
                }

                if (revealText.Equals(""))
                {
                    configurationList.Add(new Configuration
                    {
                        Category = row.Category.Content.Value, Name = row.Name.Content.Value,
                        Value = row.Value.Content.Value
                    });
                    
                }
                else
                {
                    configurationList.Add(new Configuration
                    {
                        Category = row.Category.Content.Value,
                        Name = row.Name.Content.Value,
                        Value = revealText
                    });

                }


            }
            return configurationList;
        }

    }
}




using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Laso
{
    
    public class StorageConfig
    {
        public string Account { get; set; }
        public string Key { get; set; }
    }
    public class ApiConfig
    {
        public  string Url{ get;set; }
        public  string Host{ get;set; }
        public  string Cookie { get;set; }
    }
    public class AutomationPartnrConfig
    {
        public  string Id{ get;set; }
        public  string Name{ get;set; }
        public  string Container{ get;set; }
    }

    
    

    public class TestingConfiguration
    {
        public string Environment { get; set; }

        public StorageConfig EscrowStorage{ get;set; }
        public StorageConfig ColdStorage{ get;set; }
        public StorageConfig MainInsightsStorage{ get;set; }
        public StorageConfig AutomationStorage{ get;set; }
        
        public AutomationPartnrConfig AutomationPartner { get;set; }
        public ApiConfig Api { get;set; }



    }


    [SetUpFixture]
    [TestFixture]
    public class GlobalSetUp
    {
        // public static KeyValuePair<string, string> ColdStorage;
        // public static KeyValuePair<string, string> EscrowStorage;
        // public static KeyValuePair<string, string> MainInsightsStorage;
        // public static KeyValuePair<string, string> AutomationStorage;

        
        public static TestingConfiguration TestConfiguration { get; set; }
        static GlobalSetUp()
        {
            ResolveEnvironment();
        }
 

        private static void ResolveEnvironment()
        {
            //future the default will be local, right now only develop is ready
            var environment = TestContext.Parameters.Get("Environment", "Develop");



            var configBuilder = new ConfigurationBuilder().AddJsonFile("dataPipeline." + environment + ".json").Build();
            TestConfiguration= new TestingConfiguration();
            configBuilder.Bind(TestConfiguration);
        }

    }
}

namespace Laso.Logging.Loggly
{
    public class LogglySettings
    {
        public bool Enabled { get; set; }
        public string CustomerToken { get; set; }
        public int MaxBatchSize { get; set; }
        public double BatchPeriodSeconds { get; set; }
        public string HostName { get; set; }
        public int EndpointPort { get; set; }
    }
}
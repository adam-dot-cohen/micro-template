using System.Collections.Generic;

namespace Insights.Data.Triggers
{
    public class JobState
    {
        public JobSpec Latest { get; set; }
        public List<JobSpec> Active { get; set; }
        public string Name { get; set; }
    }

    public class JobSpec
    {
        public string JobId { get; set; }
        public string Version { get; set; }
    }
}
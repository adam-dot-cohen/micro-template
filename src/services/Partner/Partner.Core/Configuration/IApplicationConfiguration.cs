using System;
using System.Collections.Generic;
using System.Text;

namespace Partner.Core.Configuration
{
    public interface IApplicationConfiguration
    {
        string QsRepositoryConnectionString { get; }
    }
}
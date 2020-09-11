using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Laso.Hosting
{
    public interface IProgram
    {
        IConfiguration GetConfiguration(string[] args, string? environment);
        IHostBuilder CreateHostBuilder(IConfiguration configuration);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Laso.Logging.AspnetSample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly ILogService _logService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, ILogService logService)
        {
            _logger = logger;
            _logService = logService;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            
            _logService.Debug("ASP.NET Core {0}","Value 1");
            _logService.Information("ASP.NET Core {0}","Value 2");
            _logService.Warning("ASP.NET Core {0}","Value 3");
            _logService.Error("ASP.NET Core {0}","Value 4");

            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}

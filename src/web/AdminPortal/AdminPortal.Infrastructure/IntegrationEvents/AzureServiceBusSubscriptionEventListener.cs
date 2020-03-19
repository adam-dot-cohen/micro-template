using System;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Laso.AdminPortal.Core.Extensions;
using Laso.AdminPortal.Infrastructure.Filters;
using Laso.AdminPortal.Infrastructure.Filters.FilterPropertyMappers;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Laso.AdminPortal.Infrastructure.IntegrationEvents
{
    public class AzureServiceBusSubscriptionEventListener<T> : BackgroundService
    {
        private readonly AzureServiceBusTopicProvider _topicProvider;
        private readonly string _subscriptionName;
        private readonly Func<T, CancellationToken, Task> _eventHandler;
        private readonly string _sqlFilter;
        private readonly ILogger<AzureServiceBusSubscriptionEventListener<T>> _logger;

        private SubscriptionClient _client;

        public AzureServiceBusSubscriptionEventListener(AzureServiceBusTopicProvider topicProvider, string subscriptionName, Func<T, CancellationToken, Task> eventHandler, Expression<Func<T, bool>> filter, ILogger<AzureServiceBusSubscriptionEventListener<T>> logger = null) : this(topicProvider, subscriptionName, eventHandler, GetSqlFilter(filter), logger) { }
        public AzureServiceBusSubscriptionEventListener(AzureServiceBusTopicProvider topicProvider, string subscriptionName, Func<T, CancellationToken, Task> eventHandler, string sqlFilter = null, ILogger<AzureServiceBusSubscriptionEventListener<T>> logger = null)
        {
            _topicProvider = topicProvider;
            _subscriptionName = subscriptionName;
            _eventHandler = eventHandler;
            _sqlFilter = sqlFilter;
            _logger = logger ?? new NullLogger<AzureServiceBusSubscriptionEventListener<T>>();
        }

        private static string GetSqlFilter(LambdaExpression filterExpression)
        {
            if (filterExpression == null)
                return null;

            //TODO: move mapper construction
            var filterExpressionHelper = new FilterExpressionHelper(
                new IFilterPropertyMapper[] {new EnumFilterPropertyMapper(), new DefaultFilterPropertyMapper() },
                new AzureServiceBusSqlFilterDialect());

            return filterExpressionHelper.GetFilter(filterExpression);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Open(stoppingToken);
        }

        public async Task Open(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() => Task.WaitAll(Close()));

            while (!stoppingToken.IsCancellationRequested && _client == null)
            {
                try
                {
                    var client = await _topicProvider.GetSubscriptionClient(typeof(T), _subscriptionName, _sqlFilter, stoppingToken);

                    var options = new MessageHandlerOptions(async x =>
                    {
                        _logger.LogCritical(x.Exception, "Exception attempting to handle message. Restarting listener.");

                        if (_client != null)
                        {
                            await Close();

                            await Open(stoppingToken);
                        }
                    })
                    {
                        AutoComplete = false,
                        MaxAutoRenewDuration = TimeSpan.FromMinutes(1),
                        MaxConcurrentCalls = 1
                    };

                    client.RegisterMessageHandler(async (x, y) =>
                    {
                        if (client.IsClosedOrClosing)
                            return;

                        try
                        {
                            var @event = JsonSerializer.Deserialize<T>(new ReadOnlySpan<byte>(x.Body));

                            await _eventHandler(@event, stoppingToken);

                            await client.CompleteAsync(x.SystemProperties.LockToken);
                        }
                        catch (Exception e)
                        {
                            _logger.LogCritical(e, "Exception attempting to handle message.");

                            try
                            {
                                if (x.SystemProperties.DeliveryCount >= 3)
                                    await client.DeadLetterAsync(x.SystemProperties.LockToken, "Exceeded retries", e.InnermostException().Message);
                            }
                            catch (Exception deadLetterException)
                            {
                                _logger.LogCritical(deadLetterException, "Exception attempting to dead letter message.");
                            }
                        }
                    }, options);

                    _client = client;
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, "Exception attempting to configure topic subscription.");

                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }
        }

        private async Task Close()
        {
            if (_client == null || _client.IsClosedOrClosing)
            {
                _client = null;

                return;
            }

            try
            {
                await _client.CloseAsync();
            }
            catch (Exception)
            {
                //TODO: logging
            }

            _client = null;
        }
    }
}

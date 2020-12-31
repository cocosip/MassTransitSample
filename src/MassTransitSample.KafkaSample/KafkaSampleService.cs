using MassTransit.KafkaIntegration;
using MassTransitSample.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MassTransitSample.KafkaSample
{
    public class KafkaSampleService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public KafkaSampleService(ILogger<KafkaSampleService> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var producer = scope.ServiceProvider.GetService<ITopicProducer<string, KafkaMessage>>();

                try
                {
                    var count = 1;
                    while (count < 100)
                    {
                        await producer.Produce(Guid.NewGuid().ToString("N"), new KafkaMessage
                        {
                            Id = Guid.NewGuid().ToString(),
                            Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")
                        });

                        Interlocked.Increment(ref count);
                    }
                    Console.ReadLine();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Publish fail!");
                }
            }

        }


        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

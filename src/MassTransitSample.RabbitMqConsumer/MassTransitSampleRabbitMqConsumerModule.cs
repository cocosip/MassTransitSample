using MassTransit;
using MassTransitSample.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace MassTransitSample.RabbitMqConsumer
{
    [DependsOn(
          typeof(MassTransitSampleCommonModule),
          typeof(AbpAutofacModule)
          )]
    public class MassTransitSampleRabbitMqConsumerModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {

            context.Services.AddMassTransit(x =>
            {
                x.AddConsumer<RabbitMqMessageConsumer>();

                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host("192.168.0.50", 5672, "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });


                    cfg.ReceiveEndpoint(RabbitMqQueues.Queue1, e =>
                    {
                        e.ConcurrentMessageLimit = 3;
                        e.Durable = true;
                        e.ExchangeType = ExchangeType.Fanout;
                        e.Consumer<RabbitMqMessageConsumer>(ctx);
                    });
                });

            }).AddMassTransitHostedService(true);

            context.Services.AddHostedService<MassTransitSampleRabbitMqConsumerHostedService>();
        }

        public override Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
        {
            var logger = context.ServiceProvider.GetRequiredService<ILogger<MassTransitSampleRabbitMqConsumerModule>>();
            var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
            var hostEnvironment = context.ServiceProvider.GetRequiredService<IHostEnvironment>();
            logger.LogInformation($"EnvironmentName => {hostEnvironment.EnvironmentName}");

            return Task.CompletedTask;
        }
    }
}

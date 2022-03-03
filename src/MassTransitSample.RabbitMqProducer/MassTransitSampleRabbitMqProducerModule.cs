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

namespace MassTransitSample.RabbitMqProducer
{
    [DependsOn(
          typeof(MassTransitSampleCommonModule),
          typeof(AbpAutofacModule)
          )]
    public class MassTransitSampleRabbitMqProducerModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            var hostEnvironment = context.Services.GetSingletonInstance<IHostEnvironment>();

            context.Services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host("192.168.0.50", 5672, "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    cfg.Message<RabbitMqMessage>(c =>
                    {
                        c.SetEntityName(RabbitMqQueues.Queue1);
                    });

                    cfg.Publish<RabbitMqMessage>(c =>
                    {
                        c.Durable = true;
                        c.ExchangeType = ExchangeType.Fanout;
                        c.AutoDelete = false;
                    });
                });
            }).AddMassTransitHostedService(true);

            context.Services.AddHostedService<MassTransitSampleRabbitMqProducerHostedService>();
        }


        public override Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
        {
            var logger = context.ServiceProvider.GetRequiredService<ILogger<MassTransitSampleRabbitMqProducerModule>>();
            var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
            var hostEnvironment = context.ServiceProvider.GetRequiredService<IHostEnvironment>();
            logger.LogInformation($"EnvironmentName => {hostEnvironment.EnvironmentName}");

            return Task.CompletedTask;
        }
    }
}

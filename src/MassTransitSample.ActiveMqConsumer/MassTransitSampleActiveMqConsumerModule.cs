using Apache.NMS.ActiveMQ.Commands;
using MassTransit;
using MassTransitSample.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace MassTransitSample.ActiveMqConsumer
{
    [DependsOn(
          typeof(MassTransitSampleCommonModule),
          typeof(AbpAutofacModule)
          )]
    public class MassTransitSampleActiveMqConsumerModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {

            context.Services.AddMassTransit(x =>
            {
                x.AddConsumer<ActiveMqMessageConsumer>();

                x.UsingActiveMq((ctx, cfg) =>
                {
                    cfg.Host("172.16.2.252", 8688, h =>
                    {
                        //h.UseSsl();

                        h.Username("admin");
                        h.Password("admin");
                    });

                    //cfg.Message<ActiveMqMessage>(c =>
                    //{
                    //    c.SetEntityName($"VirtualTopic.{ActiveMqTopic.Topic1}");
                    //});

                    cfg.ReceiveEndpoint($"{ActiveMqTopic.Topic1}", e =>
                    {
                        //e.Bind($"VirtualTopic.{ActiveMqTopic.Topic1}");
                        e.ConcurrentMessageLimit = 1;
                        //e.QueueExpiration = TimeSpan.FromSeconds(300);
                        e.PrefetchCount = 1;
                        e.Durable = true;
                        //e.AutoDelete = false;
                        //e.ExchangeType = ExchangeType.Fanout;
                        e.ConfigureConsumer<ActiveMqMessageConsumer>(ctx);
                    });
                    cfg.EnableArtemisCompatibility();
                });

            });

            context.Services
                .Configure<MassTransitHostOptions>(options =>
                {
                    // if specified, waits until the bus is started before
                    // returning from IHostedService.StartAsync
                    // default is false
                    options.WaitUntilStarted = true;

                    // if specified, limits the wait time when starting the bus
                    options.StartTimeout = TimeSpan.FromSeconds(10);

                    // if specified, limits the wait time when stopping the bus
                    options.StopTimeout = TimeSpan.FromSeconds(30);
                });

            context.Services.AddHostedService<MassTransitSampleActiveMqConsumerHostedService>();
        }

        public override Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
        {
            var logger = context.ServiceProvider.GetRequiredService<ILogger<MassTransitSampleActiveMqConsumerModule>>();
            var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
            var hostEnvironment = context.ServiceProvider.GetRequiredService<IHostEnvironment>();
            logger.LogInformation($"EnvironmentName => {hostEnvironment.EnvironmentName}");

            return Task.CompletedTask;
        }
    }
}

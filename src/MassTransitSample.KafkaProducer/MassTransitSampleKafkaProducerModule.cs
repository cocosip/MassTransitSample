using MassTransit;
using MassTransit.KafkaIntegration;
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

namespace MassTransitSample.KafkaProducer
{
    [DependsOn(
        typeof(MassTransitSampleCommonModule),
        typeof(AbpAutofacModule)
        )]
    public class MassTransitSampleKafkaProducerModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            var hostEnvironment = context.Services.GetSingletonInstance<IHostEnvironment>();

            context.Services.AddMassTransit(x =>
            {
                x.UsingInMemory((ctx, cfg) =>
                {
                    cfg.ConcurrentMessageLimit = 100;
                    cfg.ConfigureEndpoints(ctx);
                });

                x.AddRider(rider =>
                {
                    rider.AddProducer<string, KafkaMessage>(KafkaTopics.Topic1);

                    rider.UsingKafka((context, k) =>
                    {
                        k.Host("192.168.0.50:9092");

                        k.ConfigureSocket(s =>
                        {
                            s.KeepaliveEnable = true;
                        });
                    });

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

            context.Services.AddHostedService<MassTransitSampleKafkaProducerHostedService>();
        }


        public override Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
        {
            var logger = context.ServiceProvider.GetRequiredService<ILogger<MassTransitSampleKafkaProducerModule>>();
            var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
            var hostEnvironment = context.ServiceProvider.GetRequiredService<IHostEnvironment>();
            logger.LogInformation($"EnvironmentName => {hostEnvironment.EnvironmentName}");

            return Task.CompletedTask;
        }
    }
}

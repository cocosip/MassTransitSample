using Castle.Core.Configuration;
using MassTransit;
using MassTransit.KafkaIntegration;
using MassTransitSample.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
            context.Services.AddMassTransit(x =>
            {
                x.UsingInMemory((ctx, cfg) =>
                {
                    cfg.ConcurrentMessageLimit = 100;
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
            }).AddMassTransitHostedService(true);
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

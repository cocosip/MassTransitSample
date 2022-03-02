using Confluent.Kafka;
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

namespace MassTransitSample.KafkaConsumer
{
    [DependsOn(
        typeof(MassTransitSampleCommonModule),
        typeof(AbpAutofacModule)
        )]
    public class MassTransitSampleKafkaConsumerModule : AbpModule
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
                    rider.AddConsumer<KafkaMessageConsumer>();

                    rider.UsingKafka((context, k) =>
                    {
                        k.Host("192.168.0.50:9092");

                        k.ConfigureSocket(s =>
                        {
                            s.KeepaliveEnable = true;
                        });

                        k.TopicEndpoint<string, KafkaMessage>(KafkaTopics.Topic1, KafkaTopics.GroupId, e =>
                        {
                            e.MaxPollInterval = TimeSpan.FromMilliseconds(600000);
                            e.SessionTimeout = TimeSpan.FromSeconds(300);
                            e.ConcurrentMessageLimit = 3;
                            e.AutoOffsetReset = AutoOffsetReset.Earliest;
                            e.CheckpointInterval = TimeSpan.FromSeconds(20);
                            e.ConfigureConsumer<KafkaMessageConsumer>(context);
                        });
                    });

                });
            }).AddMassTransitHostedService(true);
        }

        public override Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
        {
            var logger = context.ServiceProvider.GetRequiredService<ILogger<MassTransitSampleKafkaConsumerModule>>();
            var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
            var hostEnvironment = context.ServiceProvider.GetRequiredService<IHostEnvironment>();
            logger.LogInformation($"EnvironmentName => {hostEnvironment.EnvironmentName}");

            return Task.CompletedTask;
        }
    }
}

﻿using MassTransit;
using MassTransitSample.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
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

                    cfg.Message<RabbitMqMessage>(c =>
                    {
                        c.SetEntityName(RabbitMqQueues.Exchange1);
                    });

                    cfg.ReceiveEndpoint(RabbitMqQueues.Queue1, e =>
                    {
                        //e.Bind(RabbitMqQueues.Exchange1);
                        e.ConcurrentMessageLimit = 1;
                        e.QueueExpiration = TimeSpan.FromSeconds(300);
                        e.PrefetchCount = 10;
                        e.Durable = true;
                        //e.AutoDelete = false;
                        //e.ExchangeType = ExchangeType.Fanout;
                        e.ConfigureConsumer<RabbitMqMessageConsumer>(ctx);
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

using MassTransit;
using MassTransit.ActiveMqTransport.Topology;
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

namespace MassTransitSample.ActiveMqProducer
{
    [DependsOn(
          typeof(MassTransitSampleCommonModule),
          typeof(AbpAutofacModule)
          )]
    public class MassTransitSampleActiveMqProducerModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            var hostEnvironment = context.Services.GetSingletonInstance<IHostEnvironment>();

            context.Services.AddMassTransit(x =>
            {
                x.UsingActiveMq((ctx, cfg) =>
                {
                    cfg.Host("172.16.2.252", 8688, h =>
                    {
                        h.Username("admin");
                        h.Password("admin");
                    });


                    //cfg.Message<ActiveMqMessage>(c =>
                    //{
                    //    c.SetEntityName(ActiveMqTopic.Topic1);
                    //});


                    EndpointConvention.Map<ActiveMqMessage>(new Uri($"activemq://172.16.2.252/{ActiveMqTopic.Topic1}"));

                    cfg.Publish<ActiveMqMessage>(c =>
                    {
                        c.Durable = true;
                        //c.Exclude = true;
                        c.AutoDelete = false;
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

            context.Services.AddHostedService<MassTransitSampleActiveMqProducerHostedService>();
        }


        public override Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
        {
            var logger = context.ServiceProvider.GetRequiredService<ILogger<MassTransitSampleActiveMqProducerModule>>();
            var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
            var hostEnvironment = context.ServiceProvider.GetRequiredService<IHostEnvironment>();
            logger.LogInformation($"EnvironmentName => {hostEnvironment.EnvironmentName}");

            return Task.CompletedTask;
        }
    }


}

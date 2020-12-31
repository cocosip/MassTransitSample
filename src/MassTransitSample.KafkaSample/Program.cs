using Confluent.Kafka;
using MassTransit;
using MassTransit.KafkaIntegration;
using MassTransitSample.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace MassTransitSample.KafkaSample
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args)
              .Build()
              .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services
                    .AddLogging(l => { l.AddConsole(); })
                    .AddMassTransit(x =>
                    {
                        x.UsingInMemory((context, cfg) =>
                        {
                            cfg.TransportConcurrencyLimit = 100;
                            cfg.ConfigureEndpoints(context);
                        });

                        x.AddRider(rider =>
                        {
                            rider.AddConsumer<KafkaMessageConsumer>();
                            rider.AddProducer<string, KafkaMessage>("test-topic3");

                            rider.UsingKafka((context, k) =>
                            {
                                k.Host("192.168.0.38:9092,192.168.0.39:9092,192.168.0.87:9092");

                                k.TopicEndpoint<string, KafkaMessage>("test-topic3", "consumer-test-topic2", e =>
                                {
                                    e.ConcurrencyLimit = 1;
                                    e.AutoOffsetReset = AutoOffsetReset.Earliest;
                                    e.CheckpointInterval = TimeSpan.FromSeconds(10);
                                    e.ConfigureConsumer<KafkaMessageConsumer>(context);
                                });

                            });
                        });
                    })
                    .AddMassTransitHostedService(true);

                    services.AddHostedService<KafkaSampleService>();
                })
               .UseConsoleLifetime();
        }


    }
}

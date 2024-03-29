﻿using MassTransit;
using MassTransitSample.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Threading;
using Volo.Abp.Timing;

namespace MassTransitSample.RabbitMqProducer
{
    public class RabbitMqProducerService : ISingletonDependency
    {
        static int Sequence1 = 1;
        protected ILogger Logger { get; }
        protected IServiceScopeFactory ServiceScopeFactory { get; }
        protected ICancellationTokenProvider CancellationTokenProvider { get; }
        protected IClock Clock { get; }
        public RabbitMqProducerService(
            ILogger<RabbitMqProducerService> logger,
            IServiceScopeFactory serviceScopeFactory,
            ICancellationTokenProvider cancellationTokenProvider,
            IClock clock)
        {
            Logger = logger;
            ServiceScopeFactory = serviceScopeFactory;
            CancellationTokenProvider = cancellationTokenProvider;
            Clock = clock;
        }

        public virtual void Run()
        {
            Task.Factory.StartNew(async () =>
            {
                while (!CancellationTokenProvider.Token.IsCancellationRequested)
                {
                    try
                    {
                        await PublishAsync();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "发送消息出错了,异常信息:{Message}", ex.Message);
                    }
                    await Task.Delay(5);
                }

            }, TaskCreationOptions.LongRunning);
        }


        public virtual async Task PublishAsync()
        {
            using var scope = ServiceScopeFactory.CreateScope();
            var publishEndpoint = scope.ServiceProvider.GetService<IPublishEndpoint>();

            var sequence = Interlocked.Increment(ref Sequence1);

            await publishEndpoint.Publish<RabbitMqMessage>(new RabbitMqMessage()
            {
                Sequence = sequence,
                MessageId = Guid.NewGuid().ToString("D"),
                PublishTime = Clock.Now
            });
        }

    }
}

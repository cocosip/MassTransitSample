using MassTransit;
using MassTransitSample.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MassTransitSample.KafkaSample
{
    public class KafkaMessageConsumer : IConsumer<KafkaMessage>
    {
        static int _consumCount = 0;
        private readonly ILogger _logger;

        public KafkaMessageConsumer(ILogger<KafkaMessageConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<KafkaMessage> context)
        {
            await Task.Delay(1);
            _logger.LogInformation("Content:{0}", context.Message.Text);
            Interlocked.Increment(ref _consumCount);
            Console.WriteLine("Consume message count:{0}", _consumCount);
        }
    }
}

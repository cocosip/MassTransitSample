﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;

namespace MassTransitSample.KafkaProducer
{
    public class MassTransitSampleKafkaProducerHostedService : IHostedService
    {
        private IAbpApplicationWithInternalServiceProvider _abpApplication;

        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _hostEnvironment;

        public MassTransitSampleKafkaProducerHostedService(IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            _configuration = configuration;
            _hostEnvironment = hostEnvironment;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _abpApplication = await AbpApplicationFactory.CreateAsync<MassTransitSampleKafkaProducerModule>(options =>
            {
                options.Services.ReplaceConfiguration(_configuration);
                options.Services.AddSingleton(_hostEnvironment);

                options.UseAutofac();
                options.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog());
            });

            await _abpApplication.InitializeAsync();

            var kafkaProducerService = _abpApplication.ServiceProvider.GetRequiredService<KafkaProducerService>();

            kafkaProducerService.Run();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _abpApplication.ShutdownAsync();
        }
    }

}

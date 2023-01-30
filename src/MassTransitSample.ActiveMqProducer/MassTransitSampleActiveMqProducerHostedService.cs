using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;

namespace MassTransitSample.ActiveMqProducer
{
    public class MassTransitSampleActiveMqProducerHostedService : IHostedService
    {
        private IAbpApplicationWithExternalServiceProvider _abpApplication;
        private readonly IServiceProvider _serviceProvider;
        private readonly ActiveMqProducerService _activeMqProducerService;

        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _hostEnvironment;

        public MassTransitSampleActiveMqProducerHostedService(
            IAbpApplicationWithExternalServiceProvider abpApplication,
            IServiceProvider serviceProvider,
            ActiveMqProducerService rabbitMqProducerService,
            IConfiguration configuration,
            IHostEnvironment hostEnvironment)
        {
            _abpApplication = abpApplication;
            _serviceProvider = serviceProvider;
            _activeMqProducerService = rabbitMqProducerService;

            _configuration = configuration;
            _hostEnvironment = hostEnvironment;

        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _abpApplication.InitializeAsync(_serviceProvider);
            _activeMqProducerService.Run();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _abpApplication.ShutdownAsync();
        }
    }
}

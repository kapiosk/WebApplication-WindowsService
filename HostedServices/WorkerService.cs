using WebApplicationService.Services;

namespace WebApplicationService.HostedServices
{
    public class WorkerService : BackgroundService
    {
        private readonly CounterService _counterService;
        public WorkerService(CounterService counterService)
        {
            _counterService = counterService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _counterService.IncrementCounter();
                //await File.WriteAllTextAsync($"C:\\Users\\kapiosk\\Desktop\\files\\{Guid.NewGuid()}", Guid.NewGuid().ToString(), stoppingToken);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}

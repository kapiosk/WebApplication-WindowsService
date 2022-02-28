using Cronos;
using WebApplicationService.Services;

namespace WebApplicationService.HostedServices
{
    public class WorkerService : BackgroundService
    {
        private readonly CounterService _counterService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly List<Task> _tasks;
        public WorkerService(CounterService counterService, IServiceScopeFactory scopeFactory)
        {
            _counterService = counterService;
            _scopeFactory = scopeFactory;
            _tasks = new();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                CronExpression expression = CronExpression.Parse("* * * * *");
                var waitTime = expression.GetNextOccurrence(DateTime.UtcNow) - DateTime.UtcNow;

                if (waitTime is not null)
                    await Task.Delay(Convert.ToInt32(Math.Ceiling(waitTime.Value.TotalMilliseconds)), stoppingToken);
                else
                    throw new TimeoutException("Worker wait time invalid");

                _counterService.IncrementCounter();
            }
        }
    }
}

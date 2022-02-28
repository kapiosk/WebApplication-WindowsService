using Cronos;
using WebApplicationService.Services;

namespace WebApplicationService.HostedServices
{
    public class WorkerService : BackgroundService, IDisposable, IAsyncDisposable
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

        public override void Dispose()
        {
            Task.WaitAll(_tasks.ToArray());
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await Task.WhenAll(_tasks.ToArray());
            GC.SuppressFinalize(this);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _tasks.Add(Task.Run(async () =>
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
            }, stoppingToken));
            return Task.CompletedTask;
        }
    }
}


//using eCatalog.Site.Data.Models;
//using eCatalog.Site.Services;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;

//namespace eCatalog.Site.HostedServices
//{
//    //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-5.0&tabs=visual-studio
//    //https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose
//    public class BackgroundTasks : IHostedService, IDisposable
//    {

//        private readonly List<WorkItem> _workItems;
//        private readonly IServiceScopeFactory _scopeFactory;
//        private readonly CacheHandlerService _cacheHandlerService;
//        public BackgroundTasks(IServiceScopeFactory scopeFactory, CacheHandlerService cacheHandlerService)
//        {
//            _scopeFactory = scopeFactory;
//            _cacheHandlerService = cacheHandlerService;
//            using var scope = _scopeFactory.CreateScope();
//            using var context = scope.ServiceProvider.GetRequiredService<Data.eCatalogContext>();
//            _workItems = context.WorkItems.ToList();
//        }

//        public WorkItem[] WorkItems => _workItems.ToArray();

//        public async Task ModifyWorkItem(WorkItem newWorkItem)
//        {
//            var wi = _workItems.First(w => w.WorkItemId == newWorkItem.WorkItemId);
//            wi.DelayMinutes = newWorkItem.DelayMinutes;
//            wi.IsActive = newWorkItem.IsActive;
//            await RefreshWorkItems();
//            using var scope = _scopeFactory.CreateScope();
//            using var context = scope.ServiceProvider.GetRequiredService<Data.eCatalogContext>();
//            context.WorkItems.Update(wi);
//            await context.SaveChangesAsync();
//        }

//        public void ForceStop(WorkItem workItem)
//        {
//            workItem = _workItems.First(w => w.WorkItemId == workItem.WorkItemId);
//            workItem.IsActive = false;
//            workItem.RunEnded();
//            workItem.CancellationTokenSource.Cancel();
//        }

//        public async Task ManuallyRun(WorkItem workItem, CancellationToken cancellationToken = new CancellationToken())
//        {
//            await DoWork(workItem, cancellationToken);
//        }

//        public void Dispose()
//        {
//            StopAsync().Wait();
//            GC.SuppressFinalize(this);
//        }

//        private Task RefreshWorkItems()
//        {
//            foreach (var workItem in _workItems)
//            {
//                if (workItem.Task?.Status != TaskStatus.WaitingForActivation)
//                {
//                    workItem.CancellationTokenSource = new();
//                    CancellationToken cancellationToken = workItem.CancellationTokenSource.Token;
//                    workItem.Task = Task.Run(async () =>
//                    {
//                        while (!cancellationToken.IsCancellationRequested && workItem.IsActive)
//                        {
//                            if (workItem.StartTime.HasValue)
//                            {
//                                var nextRunTime = workItem.StartTime.Value.ToUniversalTime();

//                                if (nextRunTime < DateTime.UtcNow)
//                                {
//                                    var utcStartTime = workItem.StartTime.Value.ToUniversalTime();
//                                    nextRunTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, utcStartTime.Hour, utcStartTime.Minute, utcStartTime.Second);
//                                    while (nextRunTime < DateTime.UtcNow)
//                                        nextRunTime = nextRunTime.AddMinutes(workItem.DelayMinutes);
//                                }

//                                if (nextRunTime >= DateTime.UtcNow)
//                                    await Task.Delay(nextRunTime - DateTime.UtcNow, cancellationToken);
//                            }

//                            await DoWork(workItem, cancellationToken);

//                            await Task.Delay(TimeSpan.FromMinutes(workItem.DelayMinutes), cancellationToken);
//                        }
//                    }, cancellationToken);
//                }
//            }
//            return Task.CompletedTask;
//        }

//        private async Task DoWork(WorkItem workItem, CancellationToken cancellationToken)
//        {
//            if (!workItem.IsProcessing)
//            {
//                workItem.RunStarted();
//                switch (workItem.Code)
//                {
//                    case WorkItem.WorkItemCode.SynchronizeCategories:
//                        using (var scope = _scopeFactory.CreateScope())
//                        {
//                            var nSS = scope.ServiceProvider.GetRequiredService<NopSynchronizationService>();
//                            await nSS.SynchronizeCategories(cancellationToken);
//                        }
//                        break;
//                    case WorkItem.WorkItemCode.SynchronizeProducts:
//                        using (var scope = _scopeFactory.CreateScope())
//                        {
//                            var nSS = scope.ServiceProvider.GetRequiredService<NopSynchronizationService>();
//                            await nSS.SynchronizeProducts(cancellationToken);
//                        }
//                        break;
//                    case WorkItem.WorkItemCode.CycleCache:
//                        while (!_cacheHandlerService.CacheItemsToReset.IsEmpty)
//                        {
//                            if (_cacheHandlerService.CacheItemsToReset.TryDequeue(out var key))
//                                await _cacheHandlerService.Reset(key);
//                        }
//                        while (!_cacheHandlerService.CacheItemsToClear.IsEmpty)
//                        {
//                            if (_cacheHandlerService.CacheItemsToClear.TryDequeue(out var key))
//                                await _cacheHandlerService.Clear(key);
//                        }
//                        break;
//                }
//                workItem.RunEnded();
//            }
//        }

//        public Task StartAsync(CancellationToken cancellationToken = new())
//        {
//            return RefreshWorkItems();
//        }

//        public Task StopAsync(CancellationToken cancellationToken = new())
//        {
//            foreach (var workItem in _workItems)
//            {
//                workItem.IsActive = false;
//            }
//            Task.WaitAll(_workItems.Select(wi => wi.Task).ToArray(), cancellationToken);
//            return Task.CompletedTask;
//        }
//    }
//}

namespace WebApplicationService.Interfaces
{
    public interface IWorkerItem
    {
        public Task Run(IServiceScopeFactory scopeFactory);
    }
}
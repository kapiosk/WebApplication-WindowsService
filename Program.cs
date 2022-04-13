using Microsoft.Extensions.Hosting.WindowsServices;
using WebApplicationService.HostedServices;
using WebApplicationService.Services;
//dotnet publish -c Release -r win-x64 --self-contained
//sc.exe create MyApplicationWindowsService3 binPath=C:\Users\kapiosk\source\repos\WebApplicationService\bin\Release\net6.0\win-x64\publish\WebApplicationService.exe
//sc.exe delete MyApplicationWindowsService3
WebApplicationOptions options = new()
{
    Args = args,
    ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default
};

var builder = WebApplication.CreateBuilder(options);

if (WindowsServiceHelpers.IsWindowsService())
{
    builder.Services.AddSingleton<IHostLifetime, WindowsServiceLifetime>();
    builder.Logging.AddEventLog(settings =>
    {
        if (string.IsNullOrEmpty(settings.SourceName))
        {
            settings.SourceName = builder.Environment.ApplicationName;
        }
    });
    builder.Host.UseWindowsService();
    builder.WebHost.UseUrls("http://localhost:5298"); //Default without this is 5000
}

builder.Services.AddRazorPages();

builder.Services.AddSingleton<CounterService>();
builder.Services.AddHostedService<WorkerService>();

await using var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

await app.RunAsync();
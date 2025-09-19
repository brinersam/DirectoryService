using Serilog;
using Serilog.Events;

namespace DirectoryService;

public static class ServiceRegistration
{
    public static IHostApplicationBuilder AddSerilogLogger(this IHostApplicationBuilder builder)
    {
        string seqConnectionString = builder.Configuration["Seq:Cstring"]
            ?? throw new ArgumentNullException("Seq:Cstring");

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.Debug()
            .WriteTo.Seq(seqConnectionString)
            .Enrich.WithThreadId()
            .Enrich.WithEnvironmentName()
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
            .CreateLogger();

        builder.Services.AddSerilog();
        return builder;
    }
}

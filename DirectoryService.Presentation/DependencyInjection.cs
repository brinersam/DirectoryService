using DirectoryService.Application.Features.Commands.CreateLocation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DirectoryService.Presentation;
public static class DependencyInjection
{
    public static IHostApplicationBuilder AddPresentation(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<CreateLocationHandler>();
        builder.Services.AddScoped<CreateDepartmentHandler>();
        return builder;
    }

}

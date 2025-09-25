using DirectoryService.Application.Features.Commands.CreateDepartment;
using DirectoryService.Application.Features.Commands.CreateLocation;
using DirectoryService.Application.Features.Commands.CreatePosition;
using DirectoryService.Application.Features.Commands.UpdateDepartment;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DirectoryService.Presentation;
public static class DependencyInjection
{
    public static IHostApplicationBuilder AddPresentation(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<CreateLocationHandler>();
        builder.Services.AddScoped<CreateDepartmentHandler>();
        builder.Services.AddScoped<CreatePositionHandler>();
        builder.Services.AddScoped<UpdateDepartmentLocationsHandler>();
        return builder;
    }

}

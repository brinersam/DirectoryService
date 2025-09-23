using Dapper;
using DbUp;
using DirectoryService.Application.Features.Commands.CreateLocation;
using DirectoryService.Application.Interfaces;
using DirectoryService.Domain.Models;
using DirectoryService.Infrastructure.Database.Repositories;
using DirectoryService.Infrastructure.Database.TypeHandlers;
using DirectoryService.Shared.Framework;
using DirectoryService.Shared.ModelInterfaces;
using DirectoryService.Shared.Options;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace DirectoryService.Infrastructure;
public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        return builder.EnsureDbExists()
            .RegisterJsonbConvertersDapper()
            .RegisterCustomConvertersDapper()
            .AddIDbConnection()
            .AddMigrator()
            .AddRepositories()
            .RegisterValidators();
    }

    private static IHostApplicationBuilder RegisterValidators(this IHostApplicationBuilder builder)
    {
        builder.Services.AddValidatorsFromAssemblyContaining<CreateLocationCommandValidator>();
        return builder;
    }

    private static IHostApplicationBuilder AddRepositories(this IHostApplicationBuilder builder)
    {
        builder.Services
            .AddScoped<IDepartmentRepository, DepartmentRepository>()
            .AddScoped<ILocationRepository, LocationRepository>()
            .AddScoped<IPositionRepository, PositionRepository>();

        return builder;
    }

    private static IHostApplicationBuilder RegisterCustomConvertersDapper(this IHostApplicationBuilder builder)
    {
        SqlMapper.AddTypeHandler(new PathTypeHandler());
        return builder;
    }


    private static IHostApplicationBuilder RegisterJsonbConvertersDapper(this IHostApplicationBuilder builder)
    {
        var types = typeof(Position).Assembly
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false }
                        && type.IsAssignableTo(typeof(IJsonbObject)))
            .ToList();

        var addTypeHandler = typeof(SqlMapper)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(m =>
                m.Name == nameof(SqlMapper.AddTypeHandler) &&
                m.IsGenericMethod &&
                m.GetParameters().Length == 1
            );

        if (addTypeHandler == null)
            throw new Exception("Cannot find generic AddTypeHandler<T>() method.");

        foreach (var type in types) // doesnt work for a record containing a jsonb btw
        {
            var handlerType = typeof(JsonbTypeHandler<>).MakeGenericType(type);
            var handlerInstance = Activator.CreateInstance(handlerType);

            var addTypeHandlerTyped = addTypeHandler.MakeGenericMethod(type.AsType());
            addTypeHandlerTyped.Invoke(null, new[] { handlerInstance });
        }

        // Above does this to all valueobjects tagged by IJsonbObject
        // SqlMapper.AddTypeHandler(new JsonbTypeHandler<VALUEOBJECT>());

        return builder;
    }


    private static IHostApplicationBuilder EnsureDbExists(this IHostApplicationBuilder builder)
    {
        var bdOpts = GetDbConfig(builder);
        EnsureDatabase.For.PostgresqlDatabase(bdOpts.CString);
        return builder;
    }

    private static IHostApplicationBuilder AddMigrator(this IHostApplicationBuilder builder)
    {
        var bdOpts = GetDbConfig(builder);
        var upgrader = DeployChanges.To
                .PostgresqlDatabase(bdOpts.CString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .WithTransaction()
                .LogToConsole()
                .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(result.Error);
            Console.ResetColor();
            Console.ReadLine();
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Migrations success!");
        Console.ResetColor();

        return builder;
    }

    private static IHostApplicationBuilder AddIDbConnection(this IHostApplicationBuilder builder)
    {
        var bdOpts = GetDbConfig(builder);

        builder.Services.AddScoped<IDbConnection>(sp => new NpgsqlConnection(bdOpts.CString));
        builder.Services.AddScoped<AppDb>();
        return builder;

    }

    private static OptionsPostgresql GetDbConfig(IHostApplicationBuilder builder) => builder.Configuration
                .GetSection(OptionsPostgresql.SECTION)
                .Get<OptionsPostgresql>()!;
}

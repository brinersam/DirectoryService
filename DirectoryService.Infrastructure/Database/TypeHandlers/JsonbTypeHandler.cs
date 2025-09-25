using Dapper;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using System.Text.Json;

namespace DirectoryService.Infrastructure.Database.TypeHandlers;
public class JsonbTypeHandler<T> : SqlMapper.TypeHandler<T>
    where T : class
{
    // Save to database
    public override void SetValue(IDbDataParameter parameter, T? value)
    {
        parameter.Value = JsonSerializer.Serialize(value);
        parameter.DbType = DbType.String;

        if (parameter is NpgsqlParameter npgsqlParameter)
            npgsqlParameter.NpgsqlDbType = NpgsqlDbType.Jsonb;
    }

    // Read from database
    public override T? Parse(object value)
    {
        var json = value.ToString();
        if (json is null || string.IsNullOrWhiteSpace(json))
            return null;
        return JsonSerializer.Deserialize<T>(json)!;
    }
}

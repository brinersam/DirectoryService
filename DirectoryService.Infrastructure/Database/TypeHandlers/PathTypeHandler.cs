using Dapper;
using DirectoryService.Domain.Models.Departments.ValueObject;
using Npgsql;
using NpgsqlTypes;
using System.Data;
namespace DirectoryService.Infrastructure.Database.TypeHandlers;
public class PathTypeHandler : SqlMapper.TypeHandler<DepartmentPath>
{
    // Read from database
    public override DepartmentPath? Parse(object json)
    {
        var path = json as string;
        if (path == null)
            return null;

        return DepartmentPath.Create(path).Value;
    }

    // Save to database
    public override void SetValue(IDbDataParameter parameter, DepartmentPath? path)
    {
        parameter.Value = path?.Value;
        parameter.DbType = DbType.String;

        if (parameter is NpgsqlParameter npgsqlParameter)
            npgsqlParameter.NpgsqlDbType = NpgsqlDbType.LTree;
    }
}

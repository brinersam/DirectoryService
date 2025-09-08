
namespace DirectoryService.Shared.Options;
public class OptionsPostgresql
{
    public static string SECTION = nameof(OptionsPostgresql);

    public string CString { get; init; } = "ERR_NO_POSTGRESQL_CString_SET";
}

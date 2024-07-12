using Libraries.Enums;
using LinqToDB.Mapping;

namespace Libraries.Models.Database;

[Table("Logs")]
public class LogEntry
{
    [PrimaryKey, Identity]
    public int Id { get; set; }
    public DateTime Time { get; set; }
    public LogLevel Level { get; set; }
    public string Module { get; set; }
    public string Message { get; set; }
}
using Libraries.Enums;
using LinqToDB.Mapping;

namespace Libraries.Models.Database;

public class Frame
{
    [PrimaryKey, Identity]
    public int Id { get; set; }
    public int Number { get; set; }
    public string? File_Name { get; set; }
    public FrameState State { get; set; }
    public bool Quality { get; set; }
    public string IPv4 { get; set; }
}
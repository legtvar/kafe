using System.ComponentModel.DataAnnotations.Schema;

namespace Kafe.Wma;

[Table("projectgroup")]
public class ProjectGroup
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int Position { get; set; }
}

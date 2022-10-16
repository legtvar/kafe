using System.ComponentModel.DataAnnotations.Schema;

namespace Kafe.Wma;

public class Playlist
{
    public int Id { get; set; }
    public string? Name { get; set; }
    [Column("desc")]
    public string? Description { get; set; }
    public int Position { get; set; }
}

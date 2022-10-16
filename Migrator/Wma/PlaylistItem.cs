using System.ComponentModel.DataAnnotations.Schema;

namespace Kafe.Wma;

[Table("playlistitem")]
public class PlaylistItem
{
    public int Id { get; set; }
    public int Position { get; set; }
    public Playlist? Playlist { get; set; }
    public Video? Video { get; set; }
}

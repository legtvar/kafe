using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma;

[Table("playlist", Schema = "lemma")]
public partial class Playlist
{
    public Playlist()
    {
        Items = new HashSet<Playlistitem>();
    }

    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;
    [Column("desc")]
    public string? Desc { get; set; }
    [Column("position")]
    public int? Position { get; set; }

    [InverseProperty("PlaylistNavigation")]
    public virtual ICollection<Playlistitem> Items { get; set; }
}

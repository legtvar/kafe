using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma;

[Table("playlistitem", Schema = "lemma")]
public partial class Playlistitem
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("position")]
    public int Position { get; set; }
    [Column("playlist")]
    public int Playlist { get; set; }
    [Column("video")]
    public int Video { get; set; }

    [ForeignKey("Playlist")]
    [InverseProperty(nameof(Lemma.Playlist.Items))]
    public virtual Playlist PlaylistNavigation { get; set; } = null!;
    [ForeignKey("Video")]
    [InverseProperty(nameof(Lemma.Video.Playlistitems))]
    public virtual Video VideoNavigation { get; set; } = null!;
}

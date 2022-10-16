using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma;

[Table("text", Schema = "lemma")]
[Index("Keyword", Name = "keyword_unique", IsUnique = true)]
public partial class Text
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("keyword")]
    public string Keyword { get; set; } = null!;
    [Column("text")]
    public string Text1 { get; set; } = null!;
}

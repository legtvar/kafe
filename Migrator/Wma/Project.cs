using System.ComponentModel.DataAnnotations.Schema;

namespace Kafe.Wma;

public class Project
{
    public int Id { get; set; }
    public string? Name { get; set; }
    [Column("desc")]
    public string? Description { get; set; }
    [Column("releasedate")]
    public DateTimeOffset ReleaseDate { get; set; }
    public string? Web { get; set; }
    public Person? Owner { get; set; }
    [Column("closed")]
    public bool IsClosed { get; set; }
    [Column("publicpseudosecret")]
    public bool IsPublic { get;set; }
    public ProjectGroup? Group { get; set; }
    [Column("externalauthorname")]
    public string? ExternalAuthorName { get; set; }
    [Column("externalauthoruco")]
    public string? ExternalAuthorUco { get; set; }
    [Column("externalauthormail")]
    public string? ExternalAuthorMail { get; set; }
    [Column("externalauthorphone")]
    public string? ExternalAuthorPhone { get; set; }
}

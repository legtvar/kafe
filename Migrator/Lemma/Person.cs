using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma;

[Table("persons", Schema = "lemma")]
public partial class Person
{
    public Person()
    {
        BanAdminNavigations = new HashSet<Ban>();
        BanPersonNavigations = new HashSet<Ban>();
        ProjectPeople = new HashSet<ProjectPerson>();
        Projects = new HashSet<Project>();
        RequestModifiedbyNavigations = new HashSet<Request>();
        RequestRequestedbyNavigations = new HashSet<Request>();
        Reservations = new HashSet<Reservation>();
        Videos = new HashSet<Video>();
        Votes = new HashSet<Vote>();
        Issuemasters = new HashSet<Issuemaster>();
        PermissionCategories = new HashSet<PermissionCategory>();
    }

    [Key]
    [Column("id")]
    public int Id { get; set; }
    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;
    [Column("email")]
    [StringLength(255)]
    public string Email { get; set; } = null!;
    [Column("address")]
    [StringLength(255)]
    public string? Address { get; set; }
    [Column("phone")]
    [StringLength(14)]
    public string Phone { get; set; } = null!;
    [Column("percentil")]
    public short Percentil { get; set; }
    [Column("reason")]
    [StringLength(255)]
    public string? Reason { get; set; }
    [Column("project")]
    [StringLength(255)]
    public string Project { get; set; } = null!;
    [Column("employee")]
    public bool Employee { get; set; }
    [Column("admin")]
    public bool Admin { get; set; }
    [Column("status")]
    [StringLength(255)]
    public string? Status { get; set; }
    [Column("agreement")]
    public bool Agreement { get; set; }
    [Column("agreementdate", TypeName = "timestamp without time zone")]
    public DateTime? Agreementdate { get; set; }

    [InverseProperty("AdminNavigation")]
    public virtual ICollection<Ban> BanAdminNavigations { get; set; }
    [InverseProperty("PersonNavigation")]
    public virtual ICollection<Ban> BanPersonNavigations { get; set; }
    [InverseProperty("PersonNavigation")]
    public virtual ICollection<ProjectPerson> ProjectPeople { get; set; }
    [InverseProperty("OwnerNavigation")]
    public virtual ICollection<Project> Projects { get; set; }
    [InverseProperty("ModifiedbyNavigation")]
    public virtual ICollection<Request> RequestModifiedbyNavigations { get; set; }
    [InverseProperty("RequestedbyNavigation")]
    public virtual ICollection<Request> RequestRequestedbyNavigations { get; set; }
    [InverseProperty("PersonNavigation")]
    public virtual ICollection<Reservation> Reservations { get; set; }
    [InverseProperty("ResponsiblepersonNavigation")]
    public virtual ICollection<Video> Videos { get; set; }
    [InverseProperty("PersonNavigation")]
    public virtual ICollection<Vote> Votes { get; set; }

    [ForeignKey("Person")]
    [InverseProperty("People")]
    public virtual ICollection<Issuemaster> Issuemasters { get; set; }
    [ForeignKey("Person")]
    [InverseProperty("People")]
    public virtual ICollection<PermissionCategory> PermissionCategories { get; set; }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma
{
    [Table("project", Schema = "lemma")]
    public partial class Project
    {
        public Project()
        {
            ProjectPeople = new HashSet<ProjectPerson>();
            ProjectReservationSources = new HashSet<ProjectReservationSource>();
            RoleTables = new HashSet<RoleTable>();
            Videos = new HashSet<Video>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("name", TypeName = "character varying")]
        public string Name { get; set; } = null!;
        [Column("desc")]
        public string? Desc { get; set; }
        [Column("releasedate", TypeName = "timestamp without time zone")]
        public DateTime? Releasedate { get; set; }
        [Column("web", TypeName = "character varying")]
        public string? Web { get; set; }
        [Column("owner")]
        public int Owner { get; set; }
        [Column("closed")]
        public bool? Closed { get; set; }
        [Column("publicpseudosecret")]
        public bool? Publicpseudosecret { get; set; }
        [Column("group")]
        public int? Group { get; set; }
        [Column("externalauthorname")]
        [StringLength(255)]
        public string? Externalauthorname { get; set; }
        [Column("externalauthoruco")]
        [StringLength(255)]
        public string? Externalauthoruco { get; set; }
        [Column("externalauthormail")]
        [StringLength(255)]
        public string? Externalauthormail { get; set; }
        [Column("externalauthorphone")]
        [StringLength(255)]
        public string? Externalauthorphone { get; set; }

        [ForeignKey("Group")]
        [InverseProperty("Projects")]
        public virtual Projectgroup? GroupNavigation { get; set; }
        [ForeignKey("Owner")]
        [InverseProperty("Projects")]
        public virtual Person OwnerNavigation { get; set; } = null!;
        [InverseProperty("ProjectNavigation")]
        public virtual ICollection<ProjectPerson> ProjectPeople { get; set; }
        [InverseProperty("ProjectNavigation")]
        public virtual ICollection<ProjectReservationSource> ProjectReservationSources { get; set; }
        [InverseProperty("ProjectNavigation")]
        public virtual ICollection<RoleTable> RoleTables { get; set; }
        [InverseProperty("ProjectNavigation")]
        public virtual ICollection<Video> Videos { get; set; }
    }
}

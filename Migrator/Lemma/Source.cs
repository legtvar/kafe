using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Kafe.Lemma
{
    [Table("sources", Schema = "lemma")]
    public partial class Source
    {
        public Source()
        {
            ProjectReservationSources = new HashSet<ProjectReservationSource>();
            SourcesReservations = new HashSet<SourcesReservation>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("issuemaster")]
        public int Issuemaster { get; set; }
        [Column("name")]
        [StringLength(255)]
        public string Name { get; set; } = null!;
        [Column("type")]
        public int Type { get; set; }
        [Column("collection")]
        public int? Collection { get; set; }
        [Column("cost")]
        [Precision(10, 2)]
        public decimal? Cost { get; set; }
        [Column("inventary_number")]
        [StringLength(255)]
        public string InventaryNumber { get; set; } = null!;
        [Column("manufacturing_number")]
        [StringLength(255)]
        public string ManufacturingNumber { get; set; } = null!;
        [Column("available")]
        public bool Available { get; set; }
        [Column("description")]
        public string? Description { get; set; }

        [ForeignKey("Collection")]
        [InverseProperty("Sources")]
        public virtual Collection? CollectionNavigation { get; set; }
        [ForeignKey("Issuemaster")]
        [InverseProperty("Sources")]
        public virtual Issuemaster IssuemasterNavigation { get; set; } = null!;
        [ForeignKey("Type")]
        [InverseProperty("Sources")]
        public virtual SourceType TypeNavigation { get; set; } = null!;
        [InverseProperty("SourceNavigation")]
        public virtual ICollection<ProjectReservationSource> ProjectReservationSources { get; set; }
        [InverseProperty("SourceNavigation")]
        public virtual ICollection<SourcesReservation> SourcesReservations { get; set; }
    }
}

using System.ComponentModel.DataAnnotations.Schema;

namespace Kafe.Wma;

[Table("persons")]
public class Person
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public int Percentil { get; set; }
    public string? Reason { get; set; }
    public string? Project { get; set; }
    [Column("employee")]
    public bool IsEmployee { get; set; }
    [Column("admin")]
    public bool IsAdmin { get; set; }
    public string? Status { get; set; }
    [Column("agreement")]
    public bool IsAgreed { get; set; }
    [Column("agreementdate")]
    public DateTimeOffset AgreementDate { get; set; }
}

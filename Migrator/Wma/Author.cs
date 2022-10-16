using System.ComponentModel.DataAnnotations.Schema;

namespace Kafe.Wma;

public class Author
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

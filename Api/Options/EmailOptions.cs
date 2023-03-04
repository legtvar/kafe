using System.ComponentModel.DataAnnotations;

namespace Kafe.Api.Options;

public record EmailOptions
{
    [Required]
    public string Host { get; init; } = null!;

    public int Port { get; init; } = 465; // SSL/TLS

    [Required]
    public string Username { get; init; } = null!;

    [Required]
    public string Password { get; init; } = null!;

    [Required]
    public string FromName { get; init; } = null!;

    [Required]
    public string FromAddress { get; init; } = null!;

    public string? EnvelopeSender { get; init; }
}

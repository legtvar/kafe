using System.ComponentModel.DataAnnotations;

namespace Kafe.Api.Options;

public record EmailOptions
{
    public EmailServiceType ServiceType { get; init; } = EmailServiceType.Default;

    public string Host { get; init; } = null!;

    public int Port { get; init; } = 465; // SSL/TLS

    public string? Username { get; init; } = null!;

    public string? Password { get; init; } = null!;

    public string FromName { get; init; } = null!;

    public string FromAddress { get; init; } = null!;

    public string? EnvelopeSender { get; init; }

    public string RelayUrl { get; init; } = null!;

    public string RelaySecret { get; init; } = null!;

    public string RedirectedTo { get; set; } = null!;

    public enum EmailServiceType
    {
        Default,
        Debug,
        Relayed,
        Redirected
    }
}

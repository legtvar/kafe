using Kafe.Api.Options;
using MailKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public class DefaultEmailService : IEmailService, IDisposable
{
    private readonly IOptions<EmailOptions> options;
    private readonly SmtpClient smtp;

    public DefaultEmailService(IOptions<EmailOptions> options, SmtpLogger protocolLogger)
    {
        this.options = options;
        smtp = new SmtpClient(protocolLogger);
    }

    public void Dispose()
    {
        ((IDisposable)smtp).Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task SendEmail(
        string to,
        string subject,
        string message,
        string? secretCopy = null,
        CancellationToken token = default)
    {
        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(options.Value.FromName, options.Value.FromAddress));
        var toAddress = new MailboxAddress(string.Empty, to);
        mimeMessage.To.Add(toAddress);
        mimeMessage.Subject = $"[KAFE] {subject}";
        if (secretCopy is not null)
        {
            mimeMessage.Bcc.Add(new MailboxAddress(string.Empty, secretCopy));
        }
        mimeMessage.Body = new TextPart("plain")
        {
            Text = message
        };

        try
        {
            await smtp.ConnectAsync(options.Value.Host, options.Value.Port, true, token);
        }
        catch (InvalidOperationException)
        {
            // no-op since the smtp client is already connected
        }

        try {
            
            if (!string.IsNullOrEmpty(options.Value.Username) || !string.IsNullOrEmpty(options.Value.Password))
            {
                await smtp.AuthenticateAsync(options.Value.Username, options.Value.Password, token);
            }
        }
        catch (InvalidOperationException)
        {
            // no-op since the smtp client is already authenticated
        }

        var envelopeSender = new MailboxAddress(
            options.Value.FromName,
            options.Value.EnvelopeSender ?? options.Value.FromAddress);
        await smtp.SendAsync(mimeMessage, envelopeSender, new[] { toAddress }, token);
    }
}

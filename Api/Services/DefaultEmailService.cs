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

    public DefaultEmailService(IOptions<EmailOptions> options)
    {
        this.options = options;
        smtp = new SmtpClient(new ProtocolLogger("smtp.log"));
    }

    public void Dispose()
    {
        ((IDisposable)smtp).Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task SendEmail(string to, string subject, string message, CancellationToken token = default)
    {
        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(options.Value.FromName, options.Value.FromAddress));
        var toAddress = new MailboxAddress(string.Empty, to);
        mimeMessage.To.Add(toAddress);
        mimeMessage.Subject = subject;
        mimeMessage.Body = new TextPart("plain")
        {
            Text = message
        };

        await smtp.ConnectAsync(options.Value.Host, options.Value.Port, true, token);
        await smtp.AuthenticateAsync(options.Value.Username, options.Value.Password, token);
        var envelopeSender = new MailboxAddress(
            options.Value.FromName,
            options.Value.EnvelopeSender ?? options.Value.FromAddress);
        await smtp.SendAsync(mimeMessage, envelopeSender, new[] { toAddress }, token);
        await smtp.DisconnectAsync(true, token);
    }
}

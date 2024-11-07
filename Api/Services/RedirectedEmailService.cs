using System.Threading;
using System.Threading.Tasks;
using Kafe.Api.Options;
using Microsoft.Extensions.Options;

namespace Kafe.Api.Services;

public class RedirectedEmailService : IEmailService
{
    private readonly IOptions<EmailOptions> options;
    private readonly DefaultEmailService defaultService;

    public RedirectedEmailService(
        IOptions<EmailOptions> options,
        DefaultEmailService defaultService)
    {
        this.options = options;
        this.defaultService = defaultService;
    }

    public Task SendEmail(
        string to,
        string subject,
        string message,
        string? secretCopy = null,
        CancellationToken token = default)
    {
        return defaultService.SendEmail(
            to: options.Value.RedirectedTo,
            subject: subject,
            message: message,
            secretCopy: secretCopy,
            token: token);
    }
}

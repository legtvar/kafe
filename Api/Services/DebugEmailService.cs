using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public class DebugEmailService : IEmailService
{
    private readonly ILogger<DebugEmailService> logger;

    public DebugEmailService(ILogger<DebugEmailService> logger)
    {
        this.logger = logger;
    }

    public Task SendEmail(string to, string subject, string message, string? secretCopy = null, CancellationToken token = default)
    {
        logger.LogInformation("\tTo: {To}\n\tSubject: {Subject}\n\tMessage:\n\n{Message}", 
            to,
            subject,
            message);
        return Task.CompletedTask;
    }
}

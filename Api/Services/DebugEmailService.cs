using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public class DebugEmailService : IEmailService
{
    private readonly ILogger<DebugEmailService> logger;

    public DebugEmailService(ILogger<DebugEmailService> logger)
    {
        this.logger = logger;
    }

    public Task SendEmail(string to, string subject, string message)
    {
        logger.LogInformation("\tTo: {}\n\tSubject: {}\n\tMessage:\n\n{}",
            to,
            subject,
            message);
        return Task.CompletedTask;
    }
}

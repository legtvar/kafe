using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public interface IEmailService
{
    Task SendEmail(string to, string subject, string message, string? secretCopy = null, CancellationToken token = default);
}

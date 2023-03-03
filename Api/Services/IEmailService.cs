using System.Threading.Tasks;

namespace Kafe.Api.Services;

public interface IEmailService
{
    Task SendEmail(string to, string subject, string message);
}

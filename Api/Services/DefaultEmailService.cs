using System;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public class DefaultEmailService : IEmailService
{
    public Task SendEmail(string to, string subject, string message)
    {
        throw new NotImplementedException();
    }
}

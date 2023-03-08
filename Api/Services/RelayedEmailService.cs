using Kafe.Api.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Api.Services;

public class RelayedEmailService : IEmailService, IDisposable
{
    private readonly HttpClient client;
    private readonly IOptions<EmailOptions> options;
    private readonly ILogger<RelayedEmailService> logger;

    public RelayedEmailService(
        IOptions<EmailOptions> options,
        ILogger<RelayedEmailService> logger)
    {
        this.options = options;
        this.logger = logger;
        client = new HttpClient();
    }

    public async Task SendEmail(string to, string subject, string message, string? secretCopy = null, CancellationToken token = default)
    {
        var dto = new RelayedMessageDto(to, $"[KAFE] {subject}", message, secretCopy);
        var request = new HttpRequestMessage(HttpMethod.Post, new Uri(options.Value.RelayUrl));
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        jsonOptions.Converters.Add(new JsonStringEnumConverter());
        var messageText = JsonSerializer.Serialize(dto, jsonOptions);
        request.Content = new StringContent(messageText);

        var authString = $"kafe:{options.Value.RelaySecret}";
        var authStringBase64 = Convert.ToBase64String(Encoding.ASCII.GetBytes(authString));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authStringBase64);
        var response = await client.SendAsync(request, token);

        var responseString = await response.Content.ReadAsStringAsync(token);
        logger.LogInformation("The email relay responded with:\n{}", responseString);
    }

    public void Dispose()
    {
        ((IDisposable)client).Dispose();
        GC.SuppressFinalize(this);
    }

    private record RelayedMessageDto(string To, string Subject, string Message, string? SecretCopy);
}

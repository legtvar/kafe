using System;
using System.IO;
using System.Text;
using MailKit;
using MailKit.Net.Smtp;
using Serilog;

namespace Kafe.Api.Services;

public sealed class SmtpLogger : IProtocolLogger
{
    private readonly ILogger log;
    private readonly MemoryStream stream = new();
    private readonly ProtocolLogger inner;

    public SmtpLogger(ILogger log)
    {
        this.log = log.ForContext<SmtpClient>();
        this.inner = new ProtocolLogger(stream);
    }

    public IAuthenticationSecretDetector AuthenticationSecretDetector {
        get => inner.AuthenticationSecretDetector;
        set => inner.AuthenticationSecretDetector = value; }

    public void Dispose()
    {
        inner.Dispose();
    }

    public void LogClient(byte[] buffer, int offset, int count)
    {
        stream.Seek(0, SeekOrigin.Begin);
        inner.LogClient(buffer, offset, count);
        log.Debug("Client: {Message}", ExtractMessage());
    }

    public void LogConnect(Uri uri)
    {
        log.Debug("Connecting to {Uri}.", uri);
    }

    public void LogServer(byte[] buffer, int offset, int count)
    {
        stream.Seek(0, SeekOrigin.Begin);
        inner.LogServer(buffer, offset, count);
        log.Debug("Server: {Message}.", ExtractMessage());
    }
    
    private string ExtractMessage()
    {
        var length = (int)stream.Position;
        var textBuffer = new byte[length];
        stream.Seek(0, SeekOrigin.Begin);
        stream.Read(textBuffer, 0, length);
        return Encoding.UTF8.GetString(textBuffer);
    }
}

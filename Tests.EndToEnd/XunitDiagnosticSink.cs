using System.Globalization;
using System.IO;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using Xunit.v3;

namespace Kafe.Tests.EndToEnd;

internal sealed class XunitDiagnosticSink(IMessageSink messageSink) : ILogEventSink
{
    private const string OutputTemplate
        = "[{Timestamp:HH:mm:ss.fff} {Level:u3} {SourceContext}]{NewLine}{Message:lj}{NewLine}{Exception}";

    private readonly MessageTemplateTextFormatter formatter
        = new(OutputTemplate, CultureInfo.InvariantCulture);

    public void Emit(LogEvent logEvent)
    {
        using var output = new StringWriter(CultureInfo.InvariantCulture);
        formatter.Format(logEvent, output);
        messageSink.OnMessage(new DiagnosticMessage(output.ToString()));
    }
}

using System.Globalization;

namespace Kafe.Common.Tests;

public class DiagnosticTests
{
    [Fact]
    public void Diagnostic_Generic_ToString()
    {
        var diagnostic = new Diagnostic(new GenericErrorDiagnostic());
        var message = diagnostic.ToString(CultureInfo.InvariantCulture);
        Assert.NotNull(message);
        Assert.Equal(GenericErrorDiagnostic.MessageFormat?.Invariant, message);
    }
}

namespace Kafe.Common.Tests;

public class ErrTests
{
    [Fact]
    public void ErrOfInt_Default_ReturnDefault()
    {
        Err<int> err = default;
        Assert.True(err.HasValue);
        Assert.Equal(0, err.Value);
    }

    [Fact]
    public void ErrOfObject_Default_ReturnGenericError()
    {
        Err<object> err = default;
        Assert.False(err.HasValue);
        Assert.True(err.Diagnostic.IsValid);
        Assert.Equal(DiagnosticSeverity.Error, err.Diagnostic.Severity);
        Assert.IsType<GenericErrorDiagnostic>(err.Diagnostic.Payload);
    }

    [Fact]
    public void ErrOfInvalidHrib_Default_ReturnGenericError()
    {
        Err<Hrib> err = default;
        Assert.False(err.HasValue);
        Assert.True(err.Diagnostic.IsValid);
        Assert.Equal(DiagnosticSeverity.Error, err.Diagnostic.Severity);
        Assert.IsType<GenericErrorDiagnostic>(err.Diagnostic.Payload);
    }
}



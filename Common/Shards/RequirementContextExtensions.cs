namespace Kafe;

public static class RequirementContextExtensions
{
    public static RequirementContext ReportDebug(this RequirementContext c, string id, LocalizedString message)
    {
        return c.Report(new(id, message, RequirementMessageSeverity.Debug));
    }

    public static RequirementContext ReportInformation(this RequirementContext c, string id, LocalizedString message)
    {
        return c.Report(new(id, message, RequirementMessageSeverity.Information));
    }

    public static RequirementContext ReportWarning(this RequirementContext c, string id, LocalizedString message)
    {
        return c.Report(new(id, message, RequirementMessageSeverity.Warning));
    }

    public static RequirementContext ReportError(this RequirementContext c, string id, LocalizedString message)
    {
        return c.Report(new(id, message, RequirementMessageSeverity.Error));
    }
}

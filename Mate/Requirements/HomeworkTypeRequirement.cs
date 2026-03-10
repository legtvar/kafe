namespace Kafe.Mate.Requirements;

public record HomeworkTypeRequirement(
    string HomeworkType
) : IRequirement
{
    public static string Moniker => "homework-type";
}
